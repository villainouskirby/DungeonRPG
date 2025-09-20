#if UNITY_EDITOR
using HeapExplorer;
using System;
using System.Collections.Generic;
using UnityEditor;   // Handles, Gizmos
#endif
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(SpriteRenderer))]
public class MonsterController : MonoBehaviour
{
    [SerializeField] MonsterStateIndicator stateIndicator;
    [SerializeField] MonsterAnimationHub animationHub;
    public MonsterStateIndicator StateIndicator => stateIndicator;
    public MonsterAnimationHub AnimationHub => animationHub;
    public SpriteRenderer AlertSR { get; private set; }
    [SerializeField] SpriteRenderer alertSR;
    [SerializeField] MonsterData data;
    [SerializeField] public Vector3 spawner;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] public string monster_Id;
    public LayerMask ObstacleMask => obstacleMask;
    // 캐시
    public MonsterData Data => data;
    public Monster_Info_Monster mdata;
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public Vector3 Spawner => spawner;
    public Transform Player { get; private set; }
    public MonsterStateMachine StateMachine => root;
    public float MaxHP { get; private set; }
    public float CurrentHP { get; private set; }
    public event System.Action<float, float> OnHpChanged;
    public event Action<float> OnDamaged;

    public Dictionary<string, Monster_Info_Monster> monsterDic;
    MonsterStateMachine root = new();
    MonsterContext ctx;

    bool _initialized = false;
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        if (!animationHub)
            animationHub = GetComponentInChildren<MonsterAnimationHub>(true);
        Sprite = GetComponent<SpriteRenderer>();
        Player = GameObject.FindWithTag("Player")?.transform;

        AlertSR = alertSR;
        if (AlertSR) AlertSR.gameObject.SetActive(false);

        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        monsterDic = SheetDataUtil.DicByKey(Monster_Info.Monster, x => x.Monster_id);
        Debug.Log($"[{name}] serialized spawner={spawner}", this);
    }

    public void InitAfterSpawn(string monsterId)
    {
        if (string.IsNullOrEmpty(monsterId))
        {
            Debug.LogError($"[{name}] InitAfterSpawn: monsterId is null/empty");
            return;
        }
        if (Data == null)
        {
            Debug.LogError($"[{name}] MonsterData is not assigned on prefab/instance!");
            return;
        }

        mdata = monsterDic[monsterId];

        ctx = new(this, mdata);
        ctx.hub.ResetAll();

        MaxHP = mdata.Monster_hp;
        CurrentHP = MaxHP;
        ctx.hp = MaxHP;
        OnHpChanged?.Invoke(CurrentHP, MaxHP);

        root = new MonsterStateMachine(); 
        root.ChangeState(new MonsterIdleState(ctx, root));
        _initialized = true;
    }
    void OnDisable()
    {
        _initialized = false;           // 풀로 반환될 때 등, 안전 가드
    }

    void Start()
    {
    }

    void Update() 
    {
        if (!_initialized) return; 
        root.Tick(); 
    }
    
    // 외부에서 데미지
    // 기존 시그니처 보존
    public void TakeDamage(float dmg) => TakeDamage(dmg, 0f);

    // 새 시그니처 스턴 지속시간 포함
    public void TakeDamage(float dmg, float stunSec)
    {
        ctx.hp = Mathf.Max(0, ctx.hp - dmg);
        CurrentHP = ctx.hp;
        OnHpChanged?.Invoke(CurrentHP, MaxHP);
        OnDamaged?.Invoke(dmg);
        Debug.Log($"{monster_Id} 몬스터에게 {dmg} 피해! (stun={stunSec:F2}s)");

        if (ctx.hp <= 0f)
        {
            StateMachine.ChangeState(new MonsterKilledState(ctx, StateMachine, gameObject, this));
            return;
        }

        // 스턴 적용
        if (stunSec > 0f)
        {
            // 현재 최상단이 스턴이면 갱신, 아니면 푸시
            if (StateMachine.Current is MonsterStunState stunState)
            {
                stunState.Refresh(stunSec); // 남은 시간 갱신
            }
            else
            {
                if (!data.isaggressive) // 비적대일시 때리면 도망침
                { StateMachine.PushState(new MonsterStunState(ctx, StateMachine, stunSec, goToFleeOnEnd: true)); }
                else // 적대적일시 때리면 그냥 스턴
                { StateMachine.PushState(new MonsterStunState(ctx, StateMachine, stunSec, goToFleeOnEnd: false)); }
            }
        }
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (data == null) return;
        if (ctx == null) ctx = new MonsterContext(this,mdata);   // 에디터 모드 대비

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
    void OnValidate()
    {
        if (!animationHub)
            animationHub = GetComponentInChildren<MonsterAnimationHub>(true);
    }
#endif
}
