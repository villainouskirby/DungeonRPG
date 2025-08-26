using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayerChangeState
{
    /* ---------- 이동/애니메이션 ---------- */
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Animator anim;

    [Header("이동 속도 설정 (상태별 변경은 스크립트 내에 있음)")]
    public float speed = 5f;   // 현재 이동 속도(상태별로 변동)
    private float baseMoveSpeed = 3f;

    [Header("달리기 스태미너 소모 설정")]
    [Tooltip("RunState 동안 이 간격(초)마다 스태미나를 차감")]
    public float runStaminaTickInterval = 0.25f;   // ← 소비 주기 (초)

    [Tooltip("한 번에 차감할 스태미나 양")]
    public float runStaminaCostPerTick = 2f;      // ← 소비량

    [Header("가속도 값")]
    [Tooltip("프레임당 속도 변화량 (값이 클수록 반응이 빠르고 작을수록 묵직함)")]
    public float accel = 10f;

    [Header("중량 패널티 배수(1=정상속도)")]
    [Range(0.1f, 1.0f)]
    public float weightSpeedMultiplier = 1f;

    public void SetWeightSpeedMultiplier(float m)
    {
        weightSpeedMultiplier = Mathf.Clamp(m, 0.1f, 1f);
    }
    [Header("회피 설정값")]
    public int dodgeCost = 50;    // 스태미너 소모
    public float diveTime = 0.30f; // 몸 던짐 구간 길이
    public float proneTime = 0.55f; // 땅에 엎드린 구간
    public float getUpTime = 0.45f; // 일어나기 구간
    public float invincibleTime = 0.20f; // 무적 프레임
    public float slideForce = 30f;  // 회피용
    public float getUpBoost = 2.0f;  // 일어나면서 밀어줄 속도(작으면 거의 제자리)
    [Header("회피 가속/감속 값")]
    public float diveAccel = 60f;   // Dive 때 가속
    public float slideDecel = 40f;   // Down(엎드린) → 정지
    public float getUpAccel = 50f;   // 일어날 때 밀어주는 가속
    public float getUpDecel = 35f;   // 일어난 뒤 감속

    private AttackController attackController;
    private PlayerStateMachine stateMachine;
    [SerializeField] IPlayerState nowState;

    #region Escape 내부 상태
    enum EscapePhase { None, Dive, Down, GetUp }
    EscapePhase escPhase = EscapePhase.None;
    float phaseT = 0f;          // 현재 페이즈 남은 시간
    Vector2 escDir = Vector2.zero;
    bool isInvincible = false;
    Vector2 EscapeTargetVelocity()
    {
        return escPhase switch
        {
            EscapePhase.Dive => escDir.normalized * slideForce,   // 앞으로 미끄러짐
            EscapePhase.Down => Vector2.zero,                     // 엎드려서 정지
            EscapePhase.GetUp => escDir * getUpBoost,              // 일어나는 힘
            _ => Vector2.zero
        };
    }

    float EscapeAccel()
    {
        return escPhase switch
        {
            EscapePhase.Dive => diveAccel,
            EscapePhase.Down => slideDecel,
            // GetUp 단계는 2차: 처음엔 accel, 중간부터 decel
            EscapePhase.GetUp =>
                phaseT > getUpTime * 0.5f ? getUpAccel : getUpDecel,
            _ => 0f
        };
    }
    #endregion
    public bool EscapeActive => escPhase != EscapePhase.None;

    private bool stateLocked = false; // 외부(포션 등) 잠금
    private int facingDir = 1;     // 0=Up,1=Down,2=Left,3=Right
    Vector2 prevDir = Vector2.zero;
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
        attackController = GetComponent<AttackController>();
        stateMachine = new PlayerStateMachine();
        stateMachine.ChangeState(new IdleState(this));
    }

    private void Update()
    {
        stateMachine.Update();
        UpdateByState();
        DrainStaminaWhileRunning();
        if (EscapeActive) UpdateEscape();

        //강제정지
        float hx = Input.GetAxisRaw("Horizontal");
        float hy = Input.GetAxisRaw("Vertical");
        Vector2 dir = new(hx, hy);

        if (prevDir != Vector2.zero && dir == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
        }
        prevDir = dir;
    }
    void FixedUpdate()
    {
        if (EscapeActive) return;

        // 입력 / 속도 계산
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");
        Vector2 dir = new(hx, hy);

        if (dir != Vector2.zero)
        {
            float finalSpeed = speed * weightSpeedMultiplier;
            Vector2 targetVel = dir.normalized * finalSpeed;
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVel,
                                              accel * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        bool attacking = attackController && attackController.IsInAttack;
        bool guarding = stateMachine.GetCurrentState() is GuardState;
        if (!attacking && !guarding)
        {
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
        }
        sprite.flipX = facingDir == 2;

        bool moving = rb.velocity.sqrMagnitude > 0.001f;

        UpdateAnimation(moving);
    }
    void UpdateAnimation(bool moving)
    {
        if (attackController && attackController.IsInAttack) { return; }

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
        anim.Play(clip);
    }
    private float _runStaminaTimer = 0f;
    private void DrainStaminaWhileRunning()
    {
        // 달리기 상태가 아닌 경우 타이머 리셋 후 종료
        if (stateMachine.GetCurrentState() is not RunState)
        {
            _runStaminaTimer = 0f;
            return;
        }

        if (!PlayerData.instance) return;

        _runStaminaTimer += Time.deltaTime;

        // 설정한 간격을 넘으면 소비
        if (_runStaminaTimer >= runStaminaTickInterval)
        {
            bool ok = PlayerData.instance.SpendStamina(runStaminaCostPerTick);

            _runStaminaTimer = 0f;   // 타이머 리셋

            if (!ok)
            {
                // 스태미나가 부족하면 달리기 해제
                stateMachine.ChangeState(new MoveState(this));
            }
        }
    }


    private void UpdateByState() // 상태에 따른 속력
    {
        var cur = stateMachine.GetCurrentState();
        speed = cur switch
        {
            IdleState or SneakState or NormalAttackState or GuardState => 0f,
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
        if (escPhase != EscapePhase.None)
        {
            Vector2 target = EscapeTargetVelocity();
            float accel = EscapeAccel();

            rb.velocity = Vector2.MoveTowards(rb.velocity,
                                              target,
                                              accel * Time.fixedDeltaTime);
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
    // 방향키 이동 강제 정지용 메소드
    float inputDeadZone = 0.001f;
    public Vector2 ReadMoveRaw()
    {
        float hx = Input.GetAxisRaw("Horizontal");
        float hy = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(hx) < inputDeadZone) hx = 0f;
        if (Mathf.Abs(hy) < inputDeadZone) hy = 0f;
        return new Vector2(hx, hy);
    }
    public void UnlockState() => stateLocked = false;
    #endregion
}
