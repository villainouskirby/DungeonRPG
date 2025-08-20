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

    [Header("약공격 설정")]
    [SerializeField] private float baseDamage = 10f;     // 플레이어 기본 공격력
    [SerializeField] private float[] comboRate = { 0.7f, 1.3f }; // 1 타, 2 타 배율
    [SerializeField] private float[] afterDelay = { 0.3f, 0.5f }; // 1 타, 2 타 후딜
    [SerializeField] private float comboInputTime = 0.10f;  // 후딜 끝~다음 입력 허용
    [SerializeField] private float hitboxActiveTime = 0.12f;  // 히트박스 유지 시간

    private int comboLockedDir = -1; // 1타 때 방향 ‑> 2타까지 유지
    public bool IsInAttack => isAttacking || isAttackCharging;
    [Header("약공격 -> 강공격 전환 시간")]
    [SerializeField] private float chargeThreshold = 0.30f;   // 0.3초보다 더 누르면 차징 공격 시작
    [Header("강공격 설정")]
    [SerializeField] public float maxChargeTime = 1f; // 완충 시간 Tmax
    [SerializeField] private float heavyMultiplier = 1f; // 배율 k
    [SerializeField] private float heavyRadius = 2.5f; // 범위 반경 r
    [SerializeField] private float heavyAfterDelay = 0.7f; // 후딜

    private int ChargeAttackCost = 50;
    private bool isAttacking = false;
    private bool isAttackCharging = false;
    private int comboStep = 0;
    private float nextAttackReady = 0f;
    private float chargeStart;
    private bool heavyOnCooldown = false;
    float pressTime;
    bool pressActive;

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


    // 공격 입력 
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

        // 짧게 눌렀다가 떼면 기본 콤보 공격
        if (Input.GetMouseButtonUp(0))
        {
            if (!isAttackCharging)
                DoQuickComboAttack();           // 콤보 처리

            else
                ReleaseCharging();              // 차징 발사

            pressActive = false;                // 이벤트 소비
        }
    }
    //콤보 공격
    void DoQuickComboAttack()
    {
        if (isAttacking) return;

        float now = Time.time;
        if (now < nextAttackReady) return;

        bool within = now - nextAttackReady <= comboInputTime;
        comboStep = within ? (comboStep == 1 ? 2 : 1) : 1;

        if (comboStep == 1)
            comboLockedDir = DirFromMouse();

        StartCoroutine(PerformAttack(comboStep));
    }

    // 평타 코루틴 
    private IEnumerator PerformAttack(int step)
    {
        isAttacking = true;

        float after = afterDelay[step - 1];
        pc.ChangeState(new NormalAttackState(pc, after));

        int dir = (step == 1)                // ★ 1타→comboLockedDir, 2타도 그대로
                  ? comboLockedDir
                  : comboLockedDir;
        pc.SetFacingDirection(dir);               // ← 바라보는 방향 변경 (Flip + Animator)

        string clip = AttackClipName(step, dir);
        anim.Play(clip, 0, 0f);

        Vector2 forward = FacingVector(dir);
        int dmg = Mathf.RoundToInt(baseDamage * comboRate[step - 1]);
        if (step == 1) DoThrust(dmg, transform.position, forward);
        else DoSlash(dmg, transform.position, forward);

        yield return new WaitForSeconds(hitboxActiveTime);

        nextAttackReady = Time.time + afterDelay[step - 1];
        yield return new WaitForSeconds(afterDelay[step - 1]);

        isAttacking = false;
        pc.ChangeState(new IdleState(pc));
    }

    // 강공격 차징 
    public bool TryStartCharging()
    {
        if (!PlayerData.instance || PlayerData.instance.currentStamina.Value < ChargeAttackCost )
        {
            Debug.Log("스테미너 부족");
            return false;
        }
        if (isAttacking || heavyOnCooldown) return false;

        isAttackCharging = true;
        chargeStart = Time.time;
        chargeUI.ShowAttackGauge();
        anim.SetTrigger("ChargeStart");
        return true;
    }
    // 현재 캔슬 로직은 PlayerStates 스크립트의 ChargeState 클래스에서 관리중.
    public void CancelCharging()
    {
        if (!isAttackCharging) return;
        isAttackCharging = false;
        chargeUI.HideAll();
        anim.SetTrigger("ChargeCancel");
    }
    public void ReleaseCharging()
    {
        if (!isAttackCharging) return;
        isAttackCharging = false;

        float ratio = Mathf.Clamp01((Time.time - chargeStart) / maxChargeTime);
        int damage = Mathf.RoundToInt(baseDamage * (1f + heavyMultiplier * ratio));

        int dir = DirFromMouse();
        pc.SetFacingDirection(dir);
        anim.SetInteger("Direction", dir);
        anim.SetTrigger("HeavyAttack");

        DoHeavyCircle(damage, transform.position, heavyRadius);
        heavyOnCooldown = true;
        //pc.ChangeState(new NormalAttackState(pc, after));
        PlayerData.instance?.SpendStamina(ChargeAttackCost);
        StartCoroutine(HeavyCooldown());
        chargeUI.HideAll();
    }
    private IEnumerator HeavyCooldown()
    {
        yield return new WaitForSeconds(heavyAfterDelay);
        heavyOnCooldown = false;   // 쿨타임 해제
        pc.ChangeState(new IdleState(pc));
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
    
    private void DoSlash(int dmg, Vector2 origin, Vector2 dir)

    {
        float w = 2f, l = 1f;
        Vector2 center = origin + dir * (l * .5f);
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(w, l), angleDeg);
        HashSet<MonsterBase1> done = new();
        foreach (var h in hits)
        {
            if (h.CompareTag("Monster") && h.TryGetComponent(out MonsterBase1 m) && done.Add(m))
                m.TakeDamage(dmg);
            if (h.CompareTag("Farm") && h.TryGetComponent(out ResourceNodeBase f))
                Debug.Log($"채집물 공격! {h.gameObject.name}");
        }
    }
    private void DoThrust(int dmg, Vector2 origin, Vector2 dir)
    {
        float radius = 2f, arc = 60f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);
        HashSet<MonsterBase1> done = new();
        foreach (var h in hits)
        {
            Vector2 to = (Vector2)h.transform.position - origin;
            if (h.CompareTag("Farm") && Vector2.Angle(dir, to) <= arc * .5f &&
                h.TryGetComponent(out ResourceNodeBase f))
                f.Damage(dmg);
            if (!h.CompareTag("Monster")) continue;
            if (Vector2.Angle(dir, to) <= arc * .5f &&
                h.TryGetComponent(out MonsterBase1 m) && done.Add(m))
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
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector2 origin = transform.position;
        int dirInt = pc ? pc.FacingDir : 1;
        Vector2 dir = FacingVector(dirInt);

        // Slash (직사각형) – 빨간색
        float w = 2f, l = 1f;
        Vector2 center   = origin + dir * (l * 0.5f);
        float   angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.color  = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.matrix = Matrix4x4.TRS(center,
                                      Quaternion.Euler(0, 0, angleDeg),
                                      Vector3.one);
        Gizmos.DrawCube(Vector3.zero, new Vector3(w, l, 0));
        Gizmos.matrix = old;

        // Thrust (부채꼴) – 파란색
        Handles.color = new Color(0f, 0f, 1f, 0.25f);
        float radius = 2f, arc = 60f;
        Handles.DrawSolidArc(origin, Vector3.forward,
                             Quaternion.Euler(0, 0, -arc * 0.5f) * dir,
                             arc, radius);

        // Heavy (원형) – 노란색
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(origin, heavyRadius);
    }
#endif
}
