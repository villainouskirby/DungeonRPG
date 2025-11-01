#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AttackController : MonoBehaviour, IPlayerChangeState
{
    /* ---------- 외부 연결 ---------- */
    [SerializeField] private PlayerController pc;           // 이동·방향 담당
    private static readonly int HashAttackSpeed = Animator.StringToHash("AttackSpeed");
    [Header("공격 판정(2D) - LayerMask")]
    [SerializeField] private LayerMask hitMask = ~0;    // Monster, Farm 등 맞을 레이어 지정
    [SerializeField] private bool hitTriggersOnly = true;
    // Overlap 결과 재사용 버퍼(필요시 늘려도 됨)
    static readonly Collider2D[] _overlapHits = new Collider2D[64];

    [Header("콤보 입력 허용 창(1타 종료 기준)")]
    [SerializeField, Min(0f)] private float comboWindowBefore = 0.50f; // 종료 0.5초 전부터
    [SerializeField, Min(0f)] private float comboWindowAfter = 0.10f; // 종료 0.1초 후까지
    [SerializeField] private bool useStrictComboWindow = true;          // 창 밖 입력은 무시
    [Header("약공격 설정")]
    [SerializeField] private float baseDamage = 10f;     // 플레이어 기본 공격력
    [SerializeField] private float comboBuffer = 0.5f;   // 콤보 버퍼시간
    [SerializeField] private float[] comboRate = { 0.7f, 1.3f }; // 1 타, 2 타 배율
    [SerializeField] private float combo1cost = 4f;
    [SerializeField] private float combo2cost = 6f;
    private float swingPlayRatio = 1f;
    //[Header("히트, 스테미나 소비 소비 타이밍(초)")]
    private float[] hitActivateTime = { 0.41f, 0.64f };
    [Header("평타 후딜(1타/2타)")]
    [SerializeField] private float[] afterDelay = { 0.2f, 0.4f };

    [SerializeField] private float comboInputTime = 0.10f;  // 후딜 끝~다음 입력 허용
    [SerializeField] private float hitboxActiveTime = 0.12f;  // 히트박스 유지 시간


    private float frontLiftY = 0.2f; // 밑에 바라볼 때 히트박스 올리기
    private Vector2 HitboxLiftForDir(int dir)
    {
        if (dir == 1) return new Vector2(0f, frontLiftY); // 아래를 볼 때만 위로 올림
        return Vector2.zero;
    }
    private float AdjustWidthByDir(float baseWidth, int dir)
    {
        return (dir == 0) ? baseWidth + 1f : baseWidth;
    }
    [Header("1타 슬래시(직사각형)")]
    [SerializeField, Min(0.01f)] private float slash1Width = 2.0f;  // 좌우 폭
    [SerializeField, Min(0.01f)] private float slash1Length = 1.2f;  // 전방 길이
    [SerializeField, Range(-0.5f, 0.5f)] private float slash1CenterOffset = 0.0f; // 중앙을 전/후로 미세 이동(길이에 대한 비율: -0.5~0.5 정도가 직관적)
    [Header("2타 슬래시(직사각형)")]
    [SerializeField, Min(0.01f)] private float slash2Width = 1.4f;
    [SerializeField, Min(0.01f)] private float slash2Length = 2.0f;
    [SerializeField, Range(-0.5f, 0.5f)] private float slash2CenterOffset = 0.0f;

    [Header("공격별 스턴 시간(초)")]
    [SerializeField] private float stunLight1 = 0.20f;  // 1타
    [SerializeField] private float stunLight2 = 0.35f;  // 2타
    [SerializeField] private float stunHeavy = 0.60f;  // 강공(최대)

    private int comboLockedDir = -1; // 1타 때 방향 ‑> 2타까지 유지
    public bool IsInAttack => isAttacking || isAttackCharging;

    //[Header("약공격 -> 강공격 전환 시간")]
    private float chargeThreshold = 0.30f;   // 0.3초보다 더 누르면 차징 공격 시작

    // 차징 UI 표시용 게이지 이벤트
    // duration = maxChargeTime, elapsed, ratio(0~1)
    public event Action<float> OnChargeStart;                         // duration
    public event Action<float, float, float> OnChargeProgress;        // elapsed, duration, ratio
    public event Action OnChargeEnd;
    private float chargeStart;
    public bool IsCharging => isAttackCharging;

    [Header("강공격 설정")]
    public float maxChargeTime = 1f; // 완충 시간 Tmax
    [SerializeField] private float heavyMultiplier = 1f; // 배율 k
    [SerializeField] private float heavyRadius = 2.5f; // 범위 반경 r
    [SerializeField] private float heavyCost = 30f;
    private float heavyAfterDelay = 0.7f; // 강공격 후딜

    [Header("강공격 범위 (직사각형)")]
    [SerializeField, Min(0.01f)] private float heavyWidth = 1.4f;
    [SerializeField, Min(0.01f)] private float heavyLength = 2.0f;
    [SerializeField, Range(-0.5f, 0.5f)] private float heavyCenterOffset = 0.0f;

    [Header("강공격 차징 소모")]
    [SerializeField] public float heavyChargeStaminaPerSec = 25f;
    private int previewDir = 3;
    [Header("Gizmo 색상 커스터마이즈")]
    [SerializeField] private Color gizmoSlash1Color = new Color(1f, 0f, 0f, 0.25f);
    [SerializeField] private Color gizmoSlash2Color = new Color(1f, 0f, 1f, 0.25f);
    [SerializeField] private Color gizmoheavyColor = new Color(1f, 1f, 0f, 0.15f);
    private bool isAttacking = false;
    private bool isAttackCharging = false;
    private int comboStep = 0;
    private float nextAttackReady = 0f;
    private bool heavyOnCooldown = false;
    float pressTime;
    bool pressActive;
    public bool IsInAttackAnimation => isAttacking;
    private Animator anim;
    private SpriteRenderer sprite;
    public bool HeavyOnCooldown => heavyOnCooldown;
    // Unity 
    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        if (!pc) pc = GetComponent<PlayerController>();
    }
    private void Start()
    {
    }
    private void Update()
    {
        if (!UIPopUpHandler.Instance.IsUIOpen) { HandleAttackInput(); }

        if (isAttackCharging)
        {
            float duration = Mathf.Max(0.01f, maxChargeTime);
            float elapsed = Mathf.Max(0f, Time.time - chargeStart);
            float ratio = Mathf.Clamp01(elapsed / duration);
            OnChargeProgress?.Invoke(elapsed, duration, ratio);
        }
    }
    #region 애니메이션 재생 로직
    private int DirFromMouse()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = transform.position.z - Camera.main.transform.position.z;
        Vector2 v = (Vector2)Camera.main.ScreenToWorldPoint(pos)
                    - (Vector2)transform.position;

        // X자 대각선(|x| > |y|?)을 기준으로 4분면 분리
        return Mathf.Abs(v.x) > Mathf.Abs(v.y)
               ? (v.x > 0 ? 3 : 2)   // Right / Left
               : (v.y > 0 ? 0 : 1);  // Up / Down
    }
    
    // 공격 애니메이션 클립 값 반환기
    private static string AttackClipName(int step, int dir)
    {
        // dir: 0=Up, 1=Down, 2=Left, 3=Right
        string suffix = dir switch
        {
            0 => "Back",
            1 => "Front",
            _ => "Side"
        };
        return $"Attack{step}{suffix}";   // 예: Attack1Down, Attack2Side
    }
    private static string HeavyAttackClipName(int dir)
    {
        // dir: 0=Up, 1=Down, 2=Left, 3=Right
        string suffix = dir switch
        {
            0 => "Back",
            1 => "Front",
            _ => "Side"
        };
        return $"HeavyAttack{suffix}";   // 예: HeavyAttackUp, HeavyAttackSide, HeavyAttackDown
    }
    private AnimationClip FindClip(string clipName)
    {
        if (anim.runtimeAnimatorController == null) return null;
        foreach (var c in anim.runtimeAnimatorController.animationClips)
            if (c && c.name == clipName) return c;
        return null;
    }
    private static string IdleClipName(int dir)
    {
        // dir: 0=Up, 1=Down, 2=Left, 3=Right
        string suffix = dir switch
        {
            0 => "back",
            1 => "front",
            _ => "side"
        };
        return $"Idle_{suffix}";
    }

    #endregion
        // 공격 입력
    private bool comboQueued = false;    // 후딜 중 눌린 입력 버퍼
    private float comboQueuedAt = -999f;
    private bool attackLocked = false;
    public void LockAttack()
    {
        attackLocked = true;
    }
    public void UnLockAttack()
    {
        attackLocked = false;
    }
    void HandleAttackInput()
    {
        if (PlayerManager.Instance && !PlayerManager.Instance.CanAttack)
        {
            pressActive = false;
            comboQueued = false;
            return;
        }
        if (pc._isDropping)
        {
            pc.rb.velocity = Vector2.zero;
            return;
        }
        if (pc != null && (pc.EscapeActive || pc.GetCurrentState() is PotionConsumeState))
        {
            pressActive = false;      // 누르고 있던 프레스도 취소
            comboQueued = false;      // 콤보 버퍼도 취소
            return;
        }
        if (attackLocked) return;
        if (Input.GetMouseButtonDown(0))
        {
            pressTime = Time.time;
            pressActive = true;
        }

        if (!pressActive) return;

        // 0.3초보다 길게 누르면 차징 시작
        if (!isAttackCharging &&                // 아직 차징 아님
            Input.GetMouseButton(0) &&          // 계속 누르고 있고
            Time.time - pressTime >= chargeThreshold &&
            !isAttacking)
        {
            if (PlayerManager.Instance && !PlayerManager.Instance.CanHeavyAttack)
            {
                pressActive = false;
                return;
            }
            // 상태 머신 진입 (속도 1f 유지용)
            pc.ChangeState(new ChargingState(pc));

            // 내부 차징 플래그·UI·애니메이션
            if (TryStartCharging())
                isAttackCharging = true;

            pressActive = false;                // 이벤트 소비
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!isAttackCharging)
            {
                float now = Time.time;

                // 아직 1타/2타의 후딜이 끝나기 전이면 => 콤보 입력만 버퍼링
                if (now <= nextAttackReady && now >= nextAttackReady - comboBuffer)
                {
                    comboQueued = true;
                    comboQueuedAt = now;
                }
                else
                {
                    DoQuickComboAttack();
                }
            }
            else
            {
                ReleaseCharging();
            }
            pressActive = false;
        }
    }
    //콤보 공격
    void DoQuickComboAttack()
    {
        if (isAttacking) return;

        float now = Time.time;
        if (now < nextAttackReady) return;

        bool within = false;
        if (useStrictComboWindow && comboQueuedAt > -1f)
        {
            float endT = nextAttackReady;
            within = (comboQueuedAt >= endT - comboWindowBefore) &&
                     (comboQueuedAt <= endT + comboWindowAfter);
        }
        else
        {
            within = (now >= nextAttackReady) && (now <= nextAttackReady + comboInputTime);
        }
        comboStep = within ? (comboStep == 1 ? 2 : 1) : 1;


        StartCoroutine(PerformAttack(comboStep));
    }
    public void CancelAttackBufferOnEscape()
    {
        pressActive = false;
        comboQueued = false;
        // 차징 중이었다면 UI 종료만 보장 (공격 발사는 안 됨)
        if (isAttackCharging)
        {
            isAttackCharging = false;
            OnChargeEnd?.Invoke();
        }
    }
    // 평타 코루틴 
    private IEnumerator PerformAttack(int step)
    {
        if (PlayerData.Instance.IsExhausted) yield break;
        isAttacking = true;

        PlayerData.Instance.BlockStaminaRegen(1f);

        int dir = DirFromMouse();
        pc.SetFacingDirection(dir);

        string clip = AttackClipName(step, dir);
        var c = FindClip(clip);

        float clipLen = (c != null && c.length > 0f) ? c.length : 1f;
        float playLen = Mathf.Max(0.01f, clipLen * Mathf.Clamp01(swingPlayRatio));
        float postDelay = Mathf.Max(0f, afterDelay[Mathf.Clamp(step - 1, 0, afterDelay.Length - 1)]);
        float totalCast = playLen + postDelay;

        nextAttackReady = Time.time + totalCast;
        pc.ChangeState(new NormalAttackState(pc, totalCast));
        anim.Play(clip, 0, 0f);

        // 히트 타이밍까지 대기
        float wantHitTime = (hitActivateTime != null && hitActivateTime.Length >= step)
                       ? Mathf.Max(0f, hitActivateTime[step - 1])
                       : 0f;
        float hitMoment = Mathf.Min(wantHitTime, playLen);
        if (hitMoment > 0f) yield return new WaitForSeconds(hitMoment);

        // 히트 + 스태미나 소모 (히트 시점에 맞춤)
        Vector2 forward = FacingVector(dir);
        Vector2 lift = HitboxLiftForDir(dir);
        float w1 = AdjustWidthByDir(slash1Width, dir);
        float w2 = AdjustWidthByDir(slash2Width, dir);

        int dmg = Mathf.RoundToInt(baseDamage * comboRate[step - 1]);

        if (step == 1)
        {
            DoSlashBox(dmg, transform.position, forward, w1, slash1Length, slash1CenterOffset, stunLight1, lift);
            SoundManager.Instance.PlaySound2D("SFX_PlayerAttack1");
        }
        else
        {
            DoSlashBox(dmg, transform.position, forward, w2, slash2Length, slash2CenterOffset, stunLight2, lift);
            SoundManager.Instance.PlaySound2D("SFX_PlayerAttack2");
        }
        // 여기서 바로 스태미나 차감 (히트 타이밍에 소비)
        if (step == 1)
            PlayerData.Instance.ConsumeComboAttackStamina(combo1cost, allowDebt: true);
        else
            PlayerData.Instance.ConsumeComboAttackStamina(combo2cost, allowDebt: true);

        // 본체 잔여
        // 히트 유지 시간(본체 안에서만)
        float activeAfterHit = Mathf.Min(hitboxActiveTime, Mathf.Max(0f, playLen - hitMoment));
        if (activeAfterHit > 0f) yield return new WaitForSeconds(activeAfterHit);

        // 본체 잔여 대기
        float remainedPlay = Mathf.Max(0f, playLen - hitMoment - activeAfterHit);
        if (remainedPlay > 0f) yield return new WaitForSeconds(remainedPlay);

        string idleClip = IdleClipName(dir);
        if (!string.IsNullOrEmpty(idleClip))
            anim.CrossFade(idleClip, 0.05f);

        if (postDelay > 0f) yield return new WaitForSeconds(postDelay);

        
        anim.SetFloat(HashAttackSpeed, 1f);

        if (step == 1 && comboQueued)
        {
            comboQueued = false;

            bool allow = true;

            if (useStrictComboWindow)
            {
                float endT = nextAttackReady;                       // 1타 종료 시각
                float winMin = endT - comboWindowBefore;              // 허용 구간 시작
                float winMax = endT + comboWindowAfter;               // 허용 구간 끝
                allow = (comboQueuedAt >= winMin && comboQueuedAt <= winMax);
            }

            if (allow)
            {
                isAttacking = false;
                DoQuickComboAttack(); // 여기서 2타로 진입
                yield break;
            }
        }

        isAttacking = false;
    }

    // 강공격 차징 
    public bool TryStartCharging()
    {
        if (PlayerManager.Instance && !PlayerManager.Instance.CanHeavyAttack) return false;
        if (!PlayerData.Instance)
        {
            return false;
        }
        if (isAttacking || heavyOnCooldown) return false;

        isAttackCharging = true;
        chargeStart = Time.time;
        OnChargeStart?.Invoke(Mathf.Max(0.01f, maxChargeTime));
        PlayerData.Instance.BeginChargeSpendCap(19f);
        //anim.SetTrigger("ChargeStart");
        return true;
    }
    // 현재 캔슬 로직은 PlayerStates 스크립트의 ChargeState 클래스에서 관리중.
    public void CancelCharging()
    {
        if (!isAttackCharging) return;
        isAttackCharging = false;
        OnChargeEnd?.Invoke();

        //anim.SetTrigger("ChargeCancel");
    }
    public void ReleaseCharging()
    {
        if (!isAttackCharging) return;
        isAttackCharging = false;

        OnChargeEnd?.Invoke();

        float ratio = Mathf.Clamp01((Time.time - chargeStart) / maxChargeTime);
        int damage = Mathf.RoundToInt(baseDamage * (1f + heavyMultiplier * ratio));

        int dir = DirFromMouse();
        pc.SetFacingDirection(dir);
        
        StartCoroutine(PerformHeavySlash(dir, damage));
    }
    private IEnumerator HeavyCooldown()
    {
        yield return new WaitForSeconds(heavyAfterDelay);
        heavyOnCooldown = false;   // 쿨타임 해제
        isAttacking = false;
    }

    // UI 실시간 갱신
    //private void HandleHeavyChargeUI()
    //{
    //    if (isAttackCharging)
    //        chargeUI.SetAttackRatio(Mathf.Clamp01((Time.time - chargeStart) / maxChargeTime));
    //}

    // 히트박스 계산
    private static Vector2 FacingVector(int dir) => dir switch
    {
        0 => Vector2.up,
        1 => Vector2.down,
        2 => Vector2.left,
        3 => Vector2.right,
        _ => Vector2.zero
    };
    private static float AngleDegFromDir(Vector2 dir)
    {
        // Physics2D 회전각은 Z축 도(deg)
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
        dir.Normalize();
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
    private void DoSlashBox(int dmg, Vector2 origin, Vector2 dir,
                        float width, float length, float centerOffset01, float stunSec = 0f
                        , Vector2 worldOffset = default)
    {
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
        dir.Normalize();

        // 중심 위치 = 캐릭터 앞쪽으로 (길이의 절반) + 오프셋(비율 * 길이)
        float forward = (length * 0.5f) + (centerOffset01 * length);
        Vector2 center = origin + dir * forward + worldOffset;
        float angleDeg = AngleDegFromDir(dir);
        Vector2 size = new Vector2(width, length);

        int hitCount = Physics2D.OverlapBoxNonAlloc(center, size, angleDeg, _overlapHits, hitMask);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angleDeg, hitMask);
        HashSet<MonsterController> done = new();

        for (int i = 0; i < hitCount; i++)
        {
            var col = _overlapHits[i];
            if (!col || !col.enabled) continue;

            // 트리거만 때리기 옵션
            if (hitTriggersOnly && !col.isTrigger) continue;

            // 자기 자신/플레이어의 콜라이더 무시
            if (col.transform == transform) continue;
            if (pc && col.attachedRigidbody == pc.rb) continue;

            // 몬스터 타격
            if (col.CompareTag("Monster") && col.TryGetComponent(out MonsterController m))
            {
                if (done.Add(m))
                    m.TakeDamage(dmg, stunSec);
                continue;
            }

            // 파밍 오브젝트(필요시 트리거도 허용)
            if (col.CompareTag("Farm") && col.TryGetComponent(out ResourceNodeBase f))
            {
                SoundManager.Instance.PlaySound2D("SFX_PlayerAttackOre");
                // TODO: 광석이랑 식물 구분 해야될듯
                f.Damage(dmg);
            }
        }
    }
    private IEnumerator PerformHeavySlash(int dir, int damage)
    {
        isAttacking = true;
        SoundManager.Instance.PlaySound2D("SFX_PlayerStrongAttackPerform");
        string clip = HeavyAttackClipName(dir);
        var c = FindClip(clip);
        float clipLen = (c != null && c.length > 0f) ? c.length : 1f;

        float playLen = Mathf.Max(0.01f, clipLen * Mathf.Clamp01(swingPlayRatio));

        // 2타와 동일한 히트 타임 = 0.64
        float wantHitTime = (hitActivateTime != null && hitActivateTime.Length >= 2)
                            ? Mathf.Max(0f, hitActivateTime[1])   // 0.64f
                            : 0.64f;
        float hitMoment = Mathf.Min(wantHitTime, playLen);

        // 총 시전시간 = 본체 + 강공 후딜
        float totalCast = playLen + Mathf.Max(0f, heavyAfterDelay);

        pc.ChangeState(new NormalAttackState(pc, totalCast));
        anim.Play(clip, 0, 0f);

        pc.FreezeMoveFor(totalCast);
        pc.rb.velocity = Vector2.zero;

        // 히트 지점까지 대기
        if (hitMoment > 0f) yield return new WaitForSeconds(hitMoment);

        // 사각형 히트(2타와 동일 범위) + 스태미나 로직(필요 시 여기서)
        Vector2 forward = FacingVector(dir);
        Vector2 lift = HitboxLiftForDir(dir);
        float wHeavy = AdjustWidthByDir(heavyWidth, dir);
        DoSlashBox(damage, transform.position, forward,
                   wHeavy, heavyLength, heavyCenterOffset, stunHeavy, lift);

        // 스테미나 소모
        PlayerData.Instance.ConsumeComboAttackStamina(heavyCost, allowDebt: true);

        // 히트 유지(본체 안)
        float activeAfterHit = Mathf.Min(hitboxActiveTime, Mathf.Max(0f, playLen - hitMoment));
        if (activeAfterHit > 0f) yield return new WaitForSeconds(activeAfterHit);

        // 본체 잔여
        float remainedPlay = Mathf.Max(0f, playLen - hitMoment - activeAfterHit);
        if (remainedPlay > 0f) yield return new WaitForSeconds(remainedPlay);

        // Idle로 전환 후 후딜
        string idleClip = IdleClipName(dir);
        if (!string.IsNullOrEmpty(idleClip))
            anim.CrossFade(idleClip, 0.05f);

        float postDelay = Mathf.Max(0f, heavyAfterDelay);
        nextAttackReady = Time.time + postDelay;
        if (postDelay > 0f) yield return new WaitForSeconds(postDelay);

        // 종료/콤보 처리 (강공 이후엔 콤보 없음)
        isAttacking = false;
    }

    public void ChangeState(IPlayerState s) => pc.ChangeState(s);
    public IPlayerState GetCurrentState() => pc.GetCurrentState();
    public void RestorePreviousState() => pc.RestorePreviousState();
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawAttackGizmos(always: true);
    }

    private void OnDrawGizmosSelected()
    {
        DrawAttackGizmos(always: false);
    }

    private void DrawAttackGizmos(bool always)
    {
        Vector2 origin = transform.position;

        int dirInt = (Application.isPlaying && pc != null) ? pc.FacingDir
                                                           : Mathf.Clamp(previewDir, 0, 3);
        Vector2 dir = FacingVector(dirInt);
        float angleDeg = AngleDegFromDir(dir);
        Vector2 lift = HitboxLiftForDir(dirInt);
        float w1 = AdjustWidthByDir(slash1Width, dirInt);
        float w2 = AdjustWidthByDir(slash2Width, dirInt);
        float wh = AdjustWidthByDir(heavyWidth, dirInt);
        // 1타 박스
        DrawBoxGizmo(origin + lift, dir, angleDeg, w1, slash1Length, slash1CenterOffset, gizmoSlash1Color);

        // 2타
        DrawBoxGizmo(origin + lift, dir, angleDeg, w2, slash2Length, slash2CenterOffset, gizmoSlash2Color);

        // 강공격 박스
        DrawBoxGizmo(origin + lift, dir, angleDeg, wh, heavyLength, heavyCenterOffset, gizmoheavyColor);
    }

    private void DrawBoxGizmo(Vector2 origin, Vector2 dir, float angleDeg,
                              float width, float length, float centerOffset01, Color col)
    {
        float forward = (length * 0.5f) + (centerOffset01 * length);
        Vector2 center = origin + dir.normalized * forward;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.color = col;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angleDeg), Vector3.one);
        Gizmos.DrawCube(Vector3.zero, new Vector3(width, length, 0f));
        Gizmos.matrix = old;

#if UNITY_EDITOR
        // 외곽선도 같이
        Handles.color = new Color(col.r, col.g, col.b, 0.9f);
        Vector3 sL = Quaternion.Euler(0, 0, angleDeg) * new Vector3(-width * 0.5f, -length * 0.5f, 0);
        Vector3 sR = Quaternion.Euler(0, 0, angleDeg) * new Vector3(width * 0.5f, -length * 0.5f, 0);
        Vector3 eR = Quaternion.Euler(0, 0, angleDeg) * new Vector3(width * 0.5f, length * 0.5f, 0);
        Vector3 eL = Quaternion.Euler(0, 0, angleDeg) * new Vector3(-width * 0.5f, length * 0.5f, 0);
        Handles.DrawAAPolyLine(3f,
            (Vector3)center + sL, (Vector3)center + sR, (Vector3)center + eR, (Vector3)center + eL, (Vector3)center + sL);
#endif
    }
#endif
}
