using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerChangeState
{
    /* ---------- 이동/애니메이션 ---------- */
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Animator anim;

    [Header("Movement Settings")]
    public float speed = 5f;   // 현재 이동 속도(상태별로 변동)
    private float baseMoveSpeed = 3f;
    [Tooltip("프레임당 속도 변화량 (값이 클수록 반응이 빠르고 작을수록 묵직함)")]
    public float accel = 10f;

    [Header(" Escape Settings")]
    public int dodgeCost = 50;    // 스태미너 소모
    public float diveTime = 0.30f; // 몸 던짐 구간 길이
    public float proneTime = 0.55f; // 땅에 엎드린 구간
    public float getUpTime = 0.45f; // 일어나기 구간
    public float invincibleTime = 0.20f; // 무적 프레임
    public float slideForce = 30f;  // 회피용
    public float getUpBoost = 2.0f;  // 일어나면서 밀어줄 속도(작으면 거의 제자리)

    private PlayerStateMachine stateMachine;
    [SerializeField] IPlayerState nowState;

    // Escape 내부 상태
    enum EscapePhase { None, Dive, Down, GetUp }
    EscapePhase escPhase = EscapePhase.None;
    float phaseT = 0f;          // 현재 페이즈 남은 시간
    Vector2 escDir = Vector2.zero;
    bool isInvincible = false;
    public bool EscapeActive => escPhase != EscapePhase.None;

    private bool stateLocked = false; // 외부(포션 등) 잠금
    private int facingDir = 1;     // 0=Up,1=Down,2=Left,3=Right
    public int FacingDir => facingDir;
    public void SetFacingDirection(int d)
    {
        facingDir = d;

        // 애니메이션·스프라이트도 즉시 갱신
        anim.SetInteger("Direction", d);

        if (d == 2) sprite.flipX = true;   // Right
        else if (d == 3) sprite.flipX = false;   // Left
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        stateMachine = new PlayerStateMachine();
        stateMachine.ChangeState(new IdleState(this));
    }

    private void Update()
    {
        stateMachine.Update();
        UpdateByState();
        if (EscapeActive) UpdateEscape();
        //Debug.Log(stateMachine.GetCurrentState());
    }
    void FixedUpdate()
    {
        if (EscapeActive) return;

        // 입력 / 속도 계산
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");
        Vector2 dir = new(hx, hy);

        Vector2 targetVel = dir.normalized * speed;
        rb.velocity = Vector2.MoveTowards(rb.velocity,
                                          targetVel,
                                          accel * Time.fixedDeltaTime);

        // 방향 결정
        if (dir != Vector2.zero)
        {
            // 실제 키 입력이 있을 때
            if (Mathf.Abs(hx) > Mathf.Abs(hy))
                facingDir = hx < 0 ? 2 : 3;   // 2=Left, 3=Right
            else
                facingDir = hy > 0 ? 0 : 1;   // 0=Up,   1=Down
        }
        else if (rb.velocity.sqrMagnitude > 0.0001f)
        {
            // 키는 떼었지만 아직 관성으로 움직이고 있을 때
            Vector2 v = rb.velocity;
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
                facingDir = v.x < 0 ? 2 : 3;
            else
                facingDir = v.y > 0 ? 0 : 1;
        }

        sprite.flipX = facingDir == 2;

        // ── 애니메이션 바로 재생 ──
        bool moving = rb.velocity.sqrMagnitude > 0.001f;

        string clip = moving
            ? facingDir switch
            {
                0 => "Walk_back",
                1 => "Walk_front",
                _ => "Walk_side"
            }
            : facingDir switch
            {
                0 => "Idle_back",
                1 => "Idle_front",
                _ => "Idle_side"
            };

        anim.Play(clip);   // 조건·변수 없이 즉시 전환
    }
    private void UpdateByState() // 상태에 따른 속력
    {
        var cur = stateMachine.GetCurrentState();
        speed = cur switch
        {
            IdleState or SneakState or NormalAttackState => 0f,
            SneakMoveState or ChargingState => 1f,
            MoveState => 3f,
            RunState => 5f,
            _ => speed
        };
        if (cur is IdleState or SneakState or SneakMoveState or MoveState or RunState)
        {
            // 0이면 그대로 1로, 아닐 때는 비율로 조절
            anim.speed = speed > 0.01f
                ? Mathf.Clamp(speed / baseMoveSpeed, 0.3f, 2.0f) // 하한·상한
                : 1f;                                            // Idle은 1배속
        }
        else
        {
            // 특수 모션(회피, 공격 등)은 원래 속도로
            anim.speed = 1f;
        }
    }

    #region 회피 기동 로직
    public bool TryBeginEscape()
    {
        if (!PlayerData.instance || !PlayerData.instance.SpendStamina(dodgeCost))
            return false;

        escPhase = EscapePhase.Dive;
        phaseT = diveTime;
        isInvincible = true;

        // 방향 캡처 (키 없으면 현 facing)
        escDir = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (escDir == Vector2.zero)
            escDir = facingDir switch
            { 0 => Vector2.up, 1 => Vector2.down, 2 => Vector2.left, _ => Vector2.right };

        rb.velocity = escDir.normalized * slideForce;
        anim.SetTrigger("Dive");
        return true;
    }

    // Escape 진행 업데이트 
    void UpdateEscape()
    {
        phaseT -= Time.deltaTime;

        switch (escPhase)
        {
            case EscapePhase.Dive:
                if (isInvincible && phaseT <= diveTime - invincibleTime)
                    isInvincible = false;
                if (phaseT <= 0f)
                {
                    StartDown();
                }
                break;

            case EscapePhase.Down:
                if (phaseT <= 0f)
                {
                    StartGetUp();
                }
                break;

            case EscapePhase.GetUp:
                // 전진 감속
                rb.velocity = Vector2.Lerp(escDir * getUpBoost, Vector2.zero,
                                           1f - phaseT / getUpTime);
                if (phaseT <= 0f)
                {
                    EndEscape();
                }
                break;
        }
    }

    void StartDown()
    {
        escPhase = EscapePhase.Down;
        phaseT = proneTime;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("Down");
    }

    void StartGetUp()
    {
        escPhase = EscapePhase.GetUp;
        phaseT = getUpTime;

        // 키 입력으로 방향 교정
        Vector2 dir = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (dir != Vector2.zero)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                SetFacingDirection(dir.x < 0 ? 2 : 3);
            else
                SetFacingDirection(dir.y > 0 ? 0 : 1);
        }

        escDir = facingDir switch
        { 0 => Vector2.up, 1 => Vector2.down, 2 => Vector2.left, _ => Vector2.right };

        rb.velocity = escDir * getUpBoost;
        anim.SetTrigger("GetUp");
    }

    void EndEscape()
    {
        escPhase = EscapePhase.None;
        rb.velocity = Vector2.zero;
        isInvincible = false;
    }
    #endregion

    #region 공통 메소드
    public void ChangeState(IPlayerState s) { if (!stateLocked) stateMachine.ChangeState(s); }
    public IPlayerState GetCurrentState() => stateMachine.GetCurrentState();
    public void RestorePreviousState() => stateMachine.RestorePreviousState();

    // 외부에서 상태 잠그기/풀기
    public void LockState()
    {
        if (!(stateMachine.GetCurrentState() is IdleState))
            ChangeState(new IdleState(this));
        stateLocked = true;
        rb.velocity = Vector2.zero;
    }
    public void UnlockState() => stateLocked = false;
    #endregion
}
