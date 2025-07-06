using System.Collections;
using System.Collections.Generic;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

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
}