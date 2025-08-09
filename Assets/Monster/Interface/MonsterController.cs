#if UNITY_EDITOR
using UnityEditor;   // Handles, Gizmos
#endif
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(SpriteRenderer))]
public class MonsterController : MonoBehaviour
{
    [SerializeField] MonsterData data;
    [SerializeField] Transform spawner;
    [SerializeField] LayerMask obstacleMask;

    public LayerMask ObstacleMask => obstacleMask;
    // 캐시
    public MonsterData Data => data;
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public Transform Spawner => spawner;
    public Transform Player { get; private set; }
    public MonsterStateMachine StateMachine => root;

    MonsterStateMachine root = new();
    MonsterContext ctx;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Sprite = GetComponent<SpriteRenderer>();
        Player = GameObject.FindWithTag("Player")?.transform;

        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        ctx = new MonsterContext(this);
    }

    void Start()
    {
        root.ChangeState(new MonsterIdleState(ctx, root));   // 루트 시작
    }

    void Update() => root.Tick();

    // 외부에서 데미지
    public void TakeDamage(float dmg)
    {
        ctx.Hp -= dmg;
        if (ctx.Hp <= 0) root.ChangeState(new MonsterKilledState(ctx, root));
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (data == null) return;
        if (ctx == null) ctx = new MonsterContext(this);   // 에디터 모드 대비

        Vector3 pos = transform.position;
        Vector3 forward3 = (Vector3)ctx.GetForward();
        float half = data.sightAngle * 0.5f;

        Quaternion leftRot = Quaternion.Euler(0, 0, -half);
        Quaternion rightRot = Quaternion.Euler(0, 0, half);

        Vector3 leftDir = leftRot * forward3 * data.sightDistance;
        Vector3 rightDir = rightRot * forward3 * data.sightDistance;

        /* ── FOV 채우기 & 외곽선 ───────────────────────────── */
        Handles.color = new Color(1f, 1f, 0f, 0.15f);   // 노랑, 투명 15 %
        Handles.DrawSolidArc(pos, Vector3.forward,
                             leftDir.normalized,
                             data.sightAngle,
                             data.sightDistance);

        Handles.color = new Color(1f, 1f, 0f, 0.35f);
        Handles.DrawLine(pos, pos + leftDir);
        Handles.DrawLine(pos, pos + rightDir);

        /* 전투 해제( lostDistance ) – 노란색 */
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.lostDistance);

        /* 청각 범위 - 시안색 */
        float hearRadius = data.hearRange;

        // 벽으로 인한 소리 감쇠 적용 로직 적용
        if (Application.isPlaying && Player != null)
        {
            Vector2 from = transform.position;
            Vector2 to = Player.position;
            float dist = Vector2.Distance(from, to);

            Vector2 dir = (to - from).normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir, dist, obstacleMask);

            hearRadius = Mathf.Max(0f, data.hearRange - hits.Length * data.soundObstaclePenalty);
        }

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);            // 시안
        Gizmos.DrawWireSphere(transform.position, hearRadius);
    }
#endif
}
