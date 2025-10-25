using Unity.IO.LowLevel.Unsafe;
using System.Collections;
using System.Collections.Generic;
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

    [Header("달리기 설정")]
    [Tooltip("RunState 동안 이 간격(초)마다 스태미나를 차감")]
    public float runStaminaTickInterval = 0.25f;   // ← 소비 주기 (초)
    public float runspeed = 5f;

    [Header("웅크리면서 이동하기 속도 설정")]
    public float sneakmovespeed = 1f;
    [Header("차징 공격시 이동 속도 설정")]
    public float chargemovespeed = 2f;
    [Header("가속도 값")]
    [Tooltip("프레임당 속도 변화량 (값이 클수록 반응이 빠르고 작을수록 묵직함)")]
    public float accel = 10f;

    [Header("달리기 속도/애니메이션 전이 스무딩")]
    [SerializeField, Tooltip("speed가 목표값으로 수렴하는 시간(초). 0이면 즉시 전환")]
    private float speedSmoothTime = 0.12f;
    [SerializeField, Tooltip("anim.speed가 목표값으로 수렴하는 시간(초). 0이면 즉시 전환")]
    private float animSmoothTime = 0.18f;

    float _speedSmoothVel = 0f;
    float _animSmoothVel = 0f;

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

    // 강공격 등 잠시 1초동안 움직임을 멈추는 로직, 그로기상태나 이럴때 쓸만할듯
    float _moveFreezeUntil = -1f;
    public void FreezeMoveFor(float seconds)
    {
        float until = Time.time + Mathf.Max(0f, seconds);
        if (until > _moveFreezeUntil) _moveFreezeUntil = until;
    }
    #region Escape 내부 상태
    enum EscapePhase { None, Dive, Down, GetUp }
    EscapePhase escPhase = EscapePhase.None;
    float phaseT = 0f;          // 현재 페이즈 남은 시간
    Vector2 escDir = Vector2.zero;
    public bool isInvincible = false;
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
    #region 피격 시 깜빡이는 로직
    Coroutine _hitBlinkCo;

    // MPB 헬퍼
    static void SetSpriteAlpha(SpriteRenderer sr, float a)
    {
        if (!sr) return;
        var mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(mpb);
        if (mpb == null) mpb = new MaterialPropertyBlock();

        // 현재 색을 읽어서 알파만 교체 (셰이더에 _Color가 있어야 함)
        Color baseColor = Color.white;
        if (sr.sharedMaterial && sr.sharedMaterial.HasProperty("_Color"))
            baseColor = sr.sharedMaterial.color;
        mpb.SetColor("_Color", new Color(baseColor.r, baseColor.g, baseColor.b, a));
        sr.SetPropertyBlock(mpb);
    }
    static void ResetSpriteAlpha(SpriteRenderer sr)
    {
        SetSpriteAlpha(sr, 1f);
    }

    // 피격 무적 + 알파 깜빡임
    public void StartHitInvincible(float duration = 1f, float blinkInterval = 0.1f)
    {
        if (_hitBlinkCo != null) StopCoroutine(_hitBlinkCo);
        _hitBlinkCo = StartCoroutine(HitInvincibleRoutine(duration, blinkInterval));
    }

    IEnumerator HitInvincibleRoutine(float duration, float blinkInterval)
    {
        isInvincible = true;

        float t = 0f;
        bool low = false; // false: 1.0, true: 0.35
        const float lowA = 0.35f;

        while (t < duration)
        {
            low = !low;
            SetSpriteAlpha(sprite, low ? lowA : 1f);
            yield return new WaitForSeconds(blinkInterval);
            t += blinkInterval;
        }

        ResetSpriteAlpha(sprite);
        isInvincible = false;
        _hitBlinkCo = null;
    }
    #endregion
    #region 낙하 로직
    public bool _isDropping = false;
    Coroutine _dropCo;
    public void StartDrop(int targetLayer, int targetGround, int targetHeight, float distance = 6f, float duration = 0.6f)
    {
        if (_isDropping) return;
        if (_dropCo != null) StopCoroutine(_dropCo);
        _dropCo = StartCoroutine(DropRoutine(targetLayer, targetGround, targetHeight, distance, duration));
    }

    private IEnumerator DropRoutine(int targetLayer, int targetGround, int targetHeight,float distance, float duration)
    {
        _isDropping = true;

        // 모든 이동 정지
        rb.velocity = Vector2.zero;
        attackController?.CancelAttackBufferOnEscape();

        // 바라보는 방향 Front 고정 + 낙하 애니메이션 재생
        SetFacingDirection(1);
        anim.CrossFade("Escape_front", 0.05f);

        Vector3 start = transform.position;
        Vector3 target = start + Vector3.down * Mathf.Abs(distance);
        float t = 0f;

        if (GetCurrentState() is not IdleState)
            ChangeState(new IdleState(this));

        float ori1 = HeightManager.Instance.PlayerHeight;
        HeightManager.Instance.AutoHeight = false;

        while (t < duration)
        {
            HeightManager.Instance.SetPlayerHeight(Mathf.Lerp(ori1, targetHeight, t / duration));
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            // 간단한 ease-in 곡선 (더 묵직하게 하고 싶으면 u*u*u)
            float ease = u * u;

            // 절대 좌표 강제 세팅 → 충돌 무시
            transform.position = Vector3.Lerp(start, target, ease);

            yield return null;
        }

        HeightManager.Instance.GroundLayer = targetGround;
        HeightManager.Instance.CurrentLayer = targetLayer;
        HeightManager.Instance.AutoHeight = true;

        // 착지 위치 고정
        transform.position = target;

        // 1프레임 멈춤
        yield return new WaitForSeconds(1f);

        _isDropping = false;
        _dropCo = null;

        // Idle 복귀
        rb.velocity = Vector2.zero;
        ChangeState(new IdleState(this));
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

        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStunRequested += HandleStun;
    }

    private void Update()
    {
        stateMachine.Update();
        UpdateByState();
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
        if (UIPopUpHandler.Instance.IsUIOpen) { return; }
        if (_isDropping)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 raw = ReadMoveRaw();

        if (stateMachine.GetCurrentState() is PotionConsumeState && raw == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        
        if (_moveFreezeUntil > 0f && Time.time < _moveFreezeUntil)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (EscapeActive) return;

        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");
        Vector2 dir = new(hx, hy);

        if (dir != Vector2.zero)
        {
            float finalSpeed = speed * weightSpeedMultiplier;
            float curSpeed = rb.velocity.magnitude;
            float nextSpeed = Mathf.MoveTowards(curSpeed, finalSpeed, accel * Time.fixedDeltaTime);

            Vector2 nextVel = dir.normalized * nextSpeed;
            if(_isMoveCorrect)
            {
                switch (_type)
                {
                    case StairType.LeftRight:
                        float yVel;
                        float correctYVel = (nextVel.y + nextVel.x * _moveCorrect.x);
                        if (correctYVel < 0)
                            yVel = Mathf.Max((nextVel.y + nextVel.x * _moveCorrect.x), -nextSpeed);
                        else
                            yVel = Mathf.Min((nextVel.y + nextVel.x * _moveCorrect.x), nextSpeed);
                        nextVel = new((nextVel.x) * _correctSpeed, yVel * _correctSpeed);
                        break;
                    case StairType.UpDown:
                        float xVel;
                        float correctXVel = (nextVel.x + nextVel.y * _moveCorrect.y);
                        if (correctXVel < 0)
                            xVel = Mathf.Max((nextVel.x + nextVel.y * _moveCorrect.y), -nextSpeed);
                        else
                            xVel = Mathf.Min((nextVel.x + nextVel.y * _moveCorrect.y), nextSpeed);
                        nextVel = new(xVel * _correctSpeed, (nextVel.y) * _correctSpeed);
                        break;
                }
            }

            rb.velocity = nextVel;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        bool attackingAnim = attackController && attackController.IsInAttackAnimation;
        bool guarding = stateMachine.GetCurrentState() is GuardState;
        if (!attackingAnim && !guarding)
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
        if (attackController && attackController.IsInAttackAnimation) { return; }

        var cur = stateMachine.GetCurrentState();
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
        if (cur is RunState)
        {
            clip = facingDir switch
            {
                0 => "Run_back",
                1 => "Run_front",
                _ => "Run_side"
            };
        }
        if (cur is SneakMoveState) 
        {
            clip = facingDir switch
            {
                0 => "SneakMove_back",
                1 => "SneakMove_front",
                _ => "SneakMove_side"
            };
        }
        if (cur is SneakState)
        {
            clip = facingDir switch
            {
                0 => "Sneak_back",
                1 => "Sneak_front",
                _ => "Sneak_side"
            };
        }
        if (cur is GuardState)
        {
            clip = facingDir switch
            {
                0 => "Guard_back",
                1 => "Guard_front",
                _ => "Guard_side"
            };
        }
        anim.Play(clip);
    }
    
    private void UpdateByState() // 상태에 따른 속력
    {
        var cur = stateMachine.GetCurrentState();
        float targetSpeed = cur switch
        {
            IdleState or SneakState or NormalAttackState or GuardState or StunState => 0f,
            SneakMoveState => sneakmovespeed,
            ChargingState => chargemovespeed,
            PotionConsumeState => 2f,
            MoveState => 3f,
            RunState => runspeed,
            _ => speed
        };
        float targetAnimSpeed;
        if (cur is IdleState or SneakState or SneakMoveState or MoveState or RunState)
        {
            // 0이면 그대로 1로, 아닐 때는 비율로 조절
            targetAnimSpeed = targetSpeed > 0.01f
            ? Mathf.Clamp(targetSpeed / baseMoveSpeed, 0.3f, 1f)
            : 1f; // Idle은 1배속                                           // Idle은 1배속
        }
        else
        {
            // 특수 모션(회피, 공격 등)은 원래 속도로
            targetAnimSpeed = 1f;
        }
        if (targetSpeed <= 3f)   // 달리기 상태가 아니면
        {
            speed = targetSpeed;              // 즉시 적용
            anim.speed = targetAnimSpeed;         // 1배속
            _speedSmoothVel = 0f;    // 스무딩 속도 초기화
            _animSmoothVel = 0f;
        }
        else
        {
            speed = Mathf.SmoothDamp(speed, targetSpeed, ref _speedSmoothVel, speedSmoothTime);
            anim.speed = Mathf.SmoothDamp(anim.speed, targetAnimSpeed, ref _animSmoothVel, animSmoothTime);
        }

    }
    // 스턴 로직
    private void HandleStun(float duration)
    {
        ChangeState(new StunState(this, duration));
    }
    #region 회피 기동 로직
    public bool TryBeginEscape()
    {
        if (PlayerData.Instance.IsExhausted) return false;
        if (PlayerData.Instance.currentStamina.Value < 0.999f) return false;
        PlayerData.Instance.ConsumeActionStamina(dodgeCost, allowDebt: true);
        escPhase = EscapePhase.Dive;
        phaseT = diveTime;
        isInvincible = true;

        // 방향 캡처 (키 없으면 현 facing)
        escDir = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (escDir == Vector2.zero)
            escDir = facingDir switch
            { 0 => Vector2.up, 1 => Vector2.down, 2 => Vector2.left, _ => Vector2.right };

        SoundManager.Instance.PlaySound2D("SFX_PlayerEscapeSlap");
        rb.velocity = escDir.normalized * slideForce;

        attackController?.CancelAttackBufferOnEscape();

        var clip = EscapeClipName(facingDir);
        anim.CrossFade(clip, 0.05f);
        return true;
    }
    private static string EscapeClipName(int dir)
    {
        // dir: 0=Up, 1=Down(Front), 2=Left, 3=Right
        string suffix = dir switch
        {
            0 => "back",
            1 => "front",
            _ => "side"
        };
        return $"Escape_{suffix}";
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
    }

    void StartGetUp()
    {
        SoundManager.Instance.PlaySound2D("SFX_PlayerEscapeGetup");
        escPhase = EscapePhase.GetUp;
        phaseT = getUpTime;

        // 키 입력으로 방향 교정
        Vector2 dir = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (dir != Vector2.zero)
        {
            escDir = dir.normalized;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                SetFacingDirection(dir.x < 0 ? 2 : 3);   // 좌우
            else
                SetFacingDirection(dir.y > 0 ? 0 : 1);   // 상하
        }
        else
        {
            escDir = facingDir switch
            {
                0 => Vector2.up,
                1 => Vector2.down,
                2 => Vector2.left,
                _ => Vector2.right
            };
        }

        rb.velocity = escDir * getUpBoost;
    }

    void EndEscape()
    {
        escPhase = EscapePhase.None;
        rb.velocity = Vector2.zero;
        isInvincible = false;
    }
    #endregion

    #region 공통 메소드
    public void ChangeState(IPlayerState s) 
    { 
        if (stateLocked)
        {
            if(s is IdleState)
                stateMachine.ChangeState(s);
        }
        else
            stateMachine.ChangeState(s); 
    }
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
    bool _isMoveCorrect = false;
    Vector2 _moveCorrect = Vector2.zero;
    StairType _type = StairType.LeftRight;
    float _correctSpeed = 1f;
    public void StartMoveCorrect(Vector2 moveCorrect, float speed, StairType type)
    {
        _isMoveCorrect = true;
        _moveCorrect = moveCorrect;
        _correctSpeed = speed;
        _type = type;
    }
    public void EndMoveCorrect()
    {
        _isMoveCorrect = false;
        _moveCorrect = Vector2.zero;
        _correctSpeed = 1f;
    }
    public void UnlockState() => stateLocked = false;
    #endregion
    private void OnDestroy()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStunRequested -= HandleStun;
    }
}
