#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AttackController : MonoBehaviour, IPlayerChangeState
{
    /* ---------- 외부 연결 ---------- */
    [SerializeField] private PlayerController pc;           // 이동·방향 담당
    [SerializeField] private ChargeUIController chargeUI;   // 공격 차징 UI
    private static readonly int HashAttackSpeed = Animator.StringToHash("AttackSpeed");
    [Header("공격 판정(2D) - LayerMask")]
    [SerializeField] private LayerMask hitMask = ~0;    // Monster, Farm 등 맞을 레이어 지정
    [Header("약공격 설정")]
    [SerializeField] private float baseDamage = 10f;     // 플레이어 기본 공격력
    [SerializeField] private float[] comboRate = { 0.7f, 1.3f }; // 1 타, 2 타 배율
    [Header("평타 애니 재생 시간(1타/2타), 사실상 상수. 애니메이션 시간 달라지면 여기서 수정")]
    [SerializeField] private float[] baseAnimLength = { 1.0f, 1.3f };
    [Header("평타 공격 시전 시간(1타/2타) 값이 적을수록 공격이 빨라짐, 애니메이션도 빨라짐")]
    [SerializeField] private float[] afterDelay = { 0.3f, 0.5f }; // 1 타, 2 타 후딜
    [SerializeField] private float comboInputTime = 0.10f;  // 후딜 끝~다음 입력 허용
    [SerializeField] private float hitboxActiveTime = 0.12f;  // 히트박스 유지 시간

    [Header("1타 원뿔 공격 파라미터")]
    [SerializeField, Min(0.01f)] private float thrustRadius = 1.5f;
    [SerializeField, Range(1f, 179f)] private float thrustArcDeg = 60f;
    [Header("2타 직사각형 공격 파라미터")]
    [SerializeField, Min(0.01f)] private float slashWidth = 1.0f;   // 좌우 폭
    [SerializeField, Min(0.01f)] private float slashLength = 1.8f;   // 전방 길이
    [SerializeField, Range(-0.5f, 0.5f)] private float slashCenterOffset = 0.0f;

    private int comboLockedDir = -1; // 1타 때 방향 ‑> 2타까지 유지
    public bool IsInAttack => isAttacking || isAttackCharging;
    [Header("약공격 -> 강공격 전환 시간")]
    [SerializeField] private float chargeThreshold = 0.30f;   // 0.3초보다 더 누르면 차징 공격 시작
    [Header("강공격 설정")]
    [SerializeField] public float maxChargeTime = 1f; // 완충 시간 Tmax
    [SerializeField] private float heavyMultiplier = 1f; // 배율 k
    [SerializeField] private float heavyRadius = 2.5f; // 범위 반경 r
    [SerializeField] private float heavyAfterDelay = 0.7f; // 후딜

    [Header("강공격 차징 소모")]
    [SerializeField] public float heavyChargeStaminaPerSec = 25f;
    private int previewDir = 3;
    [Header("Gizmo 색상 커스터마이즈")]
    [SerializeField] private Color gizmoSlashColor = new Color(1f, 0f, 0f, 0.25f);
    [SerializeField] private Color gizmoThrustColor = new Color(0f, 0f, 1f, 0.25f);
    [SerializeField] private Color gizmoHeavyColor = new Color(1f, 1f, 0f, 0.15f);
    private bool isAttacking = false;
    private bool isAttackCharging = false;
    private int comboStep = 0;
    private float nextAttackReady = 0f;
    private float chargeStart;
    private bool heavyOnCooldown = false;
    float pressTime;
    bool pressActive;
    public bool IsCharging => isAttackCharging;
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
        if (chargeUI) chargeUI.HideAll();
    }
    private void Update()
    {
        HandleAttackInput();
        HandleHeavyChargeUI();
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
    private static string Suffix(int d) => d switch
    {
        0 => "Up",
        1 => "Down",
        2 => "Left",
        _ => "Right"
    };
    // 공격 애니메이션 클립 값 반환기
    private static string AttackClipName(int step, int dir)
    {
        // dir: 0=Up, 1=Down, 2=Left, 3=Right
        string suffix = dir switch
        {
            0 => "Up",
            1 => "Down",
            _ => "Side"
        };
        return $"Attack{step}{suffix}";   // 예: Attack1Down, Attack2Side
    }
    private static string HeavyAttackClipName(int dir)
    {
        // dir: 0=Up, 1=Down, 2=Left, 3=Right
        string suffix = dir switch
        {
            0 => "Up",
            1 => "Down",
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
    #endregion
    // 공격 입력
    private bool comboQueued = false;    // 후딜 중 눌린 입력 버퍼
    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressTime = Time.time;
            pressActive = true;
        }

        if (!pressActive) return;

        // 0.3초보다 길게 누르면 차징 시작
        if (!isAttackCharging &&                // 아직 차징 아님
            Input.GetMouseButton(0) &&          // 계속 누르고 있고
            Time.time - pressTime >= chargeThreshold)
        {
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
                if (isAttacking || now < nextAttackReady)
                {
                    comboQueued = true;
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

        bool within = (now >= nextAttackReady) && (now <= nextAttackReady + comboInputTime);

        comboStep = within ? (comboStep == 1 ? 2 : 1) : 1;


        StartCoroutine(PerformAttack(comboStep));
    }

    // 평타 코루틴 
    private IEnumerator PerformAttack(int step)
    {
        isAttacking = true;

        float totalCast = Mathf.Max(0.05f, afterDelay[step - 1]); // 총 시전시간(=애니 길이)
        pc.ChangeState(new NormalAttackState(pc, totalCast));

        int dir = DirFromMouse();
        pc.SetFacingDirection(dir);

        string clip = AttackClipName(step, dir);

        var c = FindClip(clip);
        float baseLen = baseAnimLength[Mathf.Clamp(step - 1, 0, baseAnimLength.Length - 1)];
        float speedMul = baseLen / Mathf.Max(0.01f, totalCast);

        anim.SetFloat(HashAttackSpeed, speedMul);

        // 재생
        anim.Play(clip, 0, 0f);

        // 히트 타이밍: 필요하면 비율로 스케일
        float activeTime = Mathf.Min(hitboxActiveTime, totalCast);
        Vector2 forward = FacingVector(dir);
        int dmg = Mathf.RoundToInt(baseDamage * comboRate[step - 1]);
        if (step == 1) DoThrust(dmg, transform.position, forward);
        else DoSlash(dmg, transform.position, forward);


        yield return new WaitForSeconds(activeTime);

        nextAttackReady = Time.time + Mathf.Max(0f, totalCast - activeTime);
        float remain = Mathf.Max(0f, totalCast - activeTime);
        if (remain > 0f) yield return new WaitForSeconds(remain);

        anim.SetFloat(HashAttackSpeed, 1f);

        if (step == 1 && comboQueued && Time.time <= nextAttackReady + comboInputTime)
        {
            comboQueued = false;
            isAttacking = false;
            DoQuickComboAttack();
            yield break;
        }

        isAttacking = false;
        comboStep = (step == 2) ? 0 : comboStep;
        pc.ChangeState(new IdleState(pc));
    }

    // 강공격 차징 
    public bool TryStartCharging()
    {
        if (!PlayerData.instance || PlayerData.instance.currentStamina.Value <= 0f)
        {
            Debug.Log("스테미너 부족");
            return false;
        }
        if (isAttacking || heavyOnCooldown) return false;

        isAttackCharging = true;
        chargeStart = Time.time;
        chargeUI.ShowAttackGauge();
        //anim.SetTrigger("ChargeStart");
        return true;
    }
    // 현재 캔슬 로직은 PlayerStates 스크립트의 ChargeState 클래스에서 관리중.
    public void CancelCharging()
    {
        if (!isAttackCharging) return;
        isAttackCharging = false;
        chargeUI.HideAll();
        //anim.SetTrigger("ChargeCancel");
    }
    public void ReleaseCharging()
    {
        
        if (!isAttackCharging) return;
        isAttackCharging = false;
        float ratio = Mathf.Clamp01((Time.time - chargeStart) / maxChargeTime);
        int damage = Mathf.RoundToInt(baseDamage * (1f + heavyMultiplier * ratio));

        int dir = DirFromMouse();
        pc.SetFacingDirection(dir);

        string clip = HeavyAttackClipName(dir);
        var c = FindClip(clip);
        float baseLen = (c != null && c.length > 0f) ? c.length : 1f;
        float speedMul = baseLen / Mathf.Max(0.01f, heavyAfterDelay);

        anim.SetFloat(HashAttackSpeed, speedMul);

        anim.Play(clip, 0, 0f);

        isAttacking = true;

        pc.FreezeMoveFor(heavyAfterDelay);

        pc.rb.velocity = Vector2.zero;
        pc.ChangeState(new NormalAttackState(pc, heavyAfterDelay));

        DoHeavyCircle(damage, transform.position, heavyRadius);
        heavyOnCooldown = true;
        //pc.ChangeState(new NormalAttackState(pc, after));
        StartCoroutine(HeavyCooldown());
        anim.SetFloat(HashAttackSpeed, 1f);
        chargeUI.HideAll();
    }
    private IEnumerator HeavyCooldown()
    {
        yield return new WaitForSeconds(heavyAfterDelay);
        heavyOnCooldown = false;   // 쿨타임 해제
        isAttacking = false;
    }

    // UI 실시간 갱신
    private void HandleHeavyChargeUI()
    {
        if (isAttackCharging)
            chargeUI.SetAttackRatio(Mathf.Clamp01((Time.time - chargeStart) / maxChargeTime));
    }

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
    private void DoSlash(int dmg, Vector2 origin, Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
        dir.Normalize();

        // 박스 중심은 캐릭터 앞쪽으로 이동 (길이의 절반 + 보정치)
        float forward = (slashLength * 0.5f) + (slashCenterOffset * slashLength);
        Vector2 center = origin + dir * forward;

        float angleDeg = AngleDegFromDir(dir);
        Vector2 size = new Vector2(slashWidth, slashLength);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angleDeg, hitMask);

        HashSet<MonsterBase1> done = new();
        foreach (var h in hits)
        {
            if (!h || !h.enabled) continue;

            if (h.CompareTag("Monster") && h.TryGetComponent(out MonsterBase1 m) && done.Add(m))
                m.TakeDamage(dmg);

            if (h.CompareTag("Farm") && h.TryGetComponent(out ResourceNodeBase f))
                f.Damage(dmg);
        }

#if UNITY_EDITOR
        if (hits.Length == 0)
            Debug.Log($"[Slash] no hits  center={center} angle={angleDeg} size={size}");
#endif
    }
    private void DoThrust(int dmg, Vector2 origin, Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
        dir.Normalize();

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, thrustRadius, hitMask);
        float halfArc = thrustArcDeg * 0.5f;

        HashSet<MonsterBase1> done = new();
        foreach (var h in hits)
        {
            if (!h || !h.enabled) continue;

            Vector2 to = (Vector2)h.transform.position - origin;
            // 각도 필터 (원뿔 내부만)
            if (Vector2.Angle(dir, to) > halfArc) continue;

            if (h.CompareTag("Farm") && h.TryGetComponent(out ResourceNodeBase f))
                f.Damage(dmg);

            if (h.CompareTag("Monster") && h.TryGetComponent(out MonsterBase1 m) && done.Add(m))
                m.TakeDamage(dmg);
        }
    }
    private void DoHeavyCircle(int dmg, Vector2 origin, float r)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, r);
        HashSet<MonsterBase1> done = new();
        foreach (var h in hits)
            if (h.CompareTag("Monster") && h.TryGetComponent(out MonsterBase1 m) && done.Add(m))
                m.TakeDamage(dmg);
    }

    public void ChangeState(IPlayerState s) => pc.ChangeState(s);
    public IPlayerState GetCurrentState() => pc.GetCurrentState();
    public void RestorePreviousState() => pc.RestorePreviousState();
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawAttackGizmos(selectedOnly: false);
    }
    private void OnDrawGizmosSelected()
    {
        DrawAttackGizmos(selectedOnly: true);
    }

    private void DrawAttackGizmos(bool selectedOnly)
    {
        Vector2 origin = transform.position;

        int dirInt;
        if (Application.isPlaying && pc != null) dirInt = pc.FacingDir;
        else dirInt = Mathf.Clamp(previewDir, 0, 3);

        Vector2 dir = FacingVector(dirInt);
        float angleDeg = AngleDegFromDir(dir);

        // 2타 직사각형 공격
        {
            float forward = (slashLength * 0.5f) + (slashCenterOffset * slashLength);
            Vector2 center = origin + dir * forward;
            Vector3 size3 = new Vector3(slashWidth, slashLength, 0f);

            Matrix4x4 old = Gizmos.matrix;
            Gizmos.color = gizmoSlashColor;
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angleDeg), Vector3.one);
            Gizmos.DrawCube(Vector3.zero, size3);
            Gizmos.matrix = old;

            // 외곽선(시각화 보조)
#if UNITY_EDITOR
            Handles.color = new Color(gizmoSlashColor.r, gizmoSlashColor.g, gizmoSlashColor.b, 0.8f);
            Vector3 p0 = center + (Vector2)(Quaternion.Euler(0, 0, angleDeg) * new Vector2(-slashWidth * 0.5f, -slashLength * 0.5f));
            Vector3 p1 = center + (Vector2)(Quaternion.Euler(0, 0, angleDeg) * new Vector2(slashWidth * 0.5f, -slashLength * 0.5f));
            Vector3 p2 = center + (Vector2)(Quaternion.Euler(0, 0, angleDeg) * new Vector2(slashWidth * 0.5f, slashLength * 0.5f));
            Vector3 p3 = center + (Vector2)(Quaternion.Euler(0, 0, angleDeg) * new Vector2(-slashWidth * 0.5f, slashLength * 0.5f));
            Handles.DrawAAPolyLine(3f, new Vector3[] { p0, p1, p2, p3, p0 });
#endif
        }

        //1타 원뿔 공격
#if UNITY_EDITOR
        {
            Handles.color = gizmoThrustColor;
            float arc = thrustArcDeg;
            // 시작방향 = dir을 -arc/2만큼 회전
            Vector3 startDir = Quaternion.Euler(0, 0, -arc * 0.5f) * (Vector3)dir;
            Handles.DrawSolidArc(origin, Vector3.forward, startDir, arc, thrustRadius);

            // 외곽선
            Handles.color = new Color(gizmoThrustColor.r, gizmoThrustColor.g, gizmoThrustColor.b, 0.9f);
            Handles.DrawWireArc(origin, Vector3.forward, startDir, arc, thrustRadius);
            Handles.DrawLine((Vector3)origin,(Vector3)(origin + dir.normalized * thrustRadius));
        }
#endif

        // 강공격 원형 공격
        Gizmos.color = gizmoHeavyColor;
        Gizmos.DrawSphere(origin, heavyRadius);
    }
#endif
}
