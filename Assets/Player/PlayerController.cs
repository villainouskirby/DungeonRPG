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

    /* ---------- Escape 내부 상태 ---------- */
    enum EscapePhase { None, Dive, Down, GetUp }
    EscapePhase escPhase = EscapePhase.None;
    float phaseT = 0f;          // 현재 페이즈 남은 시간
    Vector2 escDir = Vector2.zero;
    bool isInvincible = false;
    public bool EscapeActive => escPhase != EscapePhase.None;

    /* ---------- 내부 ---------- */
    private bool stateLocked = false; // 외부(포션 등) 잠금
    private int facingDir = 1;     // 0=Up,1=Down,2=Left,3=Right
    public int FacingDir => facingDir;
    public void SetFacingDirection(int d)
    {
        facingDir = d;

        // 애니메이션·스프라이트도 즉시 갱신
        anim.SetInteger("Direction", d);

        if (d == 3) sprite.flipX = true;   // Right
        else if (d == 2) sprite.flipX = false;   // Left
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
        Debug.Log(stateMachine.GetCurrentState());
    }
    private void FixedUpdate()
    {
        if (EscapeActive) return;
        /* 이동 */
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");

        if (hx == 0f && hy == 0f) { rb.velocity = Vector2.zero; return; }

        Vector2 dir = new(hx, hy);
        rb.velocity = dir.normalized * speed;
        anim.SetBool("iswalking", true);

        if (Mathf.Abs(hx) > 0f) facingDir = (hx < 0) ? 2 : 3;
        else if (Mathf.Abs(hy) > 0f) facingDir = (hy > 0) ? 0 : 1;

        anim.SetInteger("Direction", facingDir);
        if (facingDir == 3) sprite.flipX = true;
        else if (facingDir == 2) sprite.flipX = false;
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
