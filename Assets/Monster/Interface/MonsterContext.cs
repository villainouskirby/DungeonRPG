using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterContext
{
    public readonly MonsterData data;
    public readonly Transform transform;
    public readonly NavMeshAgent agent;
    public readonly Animator anim;
    public readonly SpriteRenderer sr;
    public readonly Transform spawner;
    public readonly Transform player;
    public readonly LayerMask obstacleMask;
    public readonly MonsterController mono;
    public readonly MonsterStateMachine sm;
    public float Hp;
    public Vector3 LastHeardPos;
    public bool IsFastReturn;


    public MonsterContext(MonsterController owner)
    {
        mono = owner;
        sm = owner.StateMachine;
        data = owner.Data;
        transform = owner.transform;
        agent = owner.Agent;
        anim = owner.Animator;
        sr = owner.Sprite;
        spawner = owner.Spawner;
        player = owner.Player;
        Hp = data.maxHp;
        obstacleMask = owner.ObstacleMask;
    }

    #region 몬스터 시야 / 청각 탐지 로직
    // 시야 확인 (벽 Raycast 포함)
    public bool CanSeePlayer(float maxDist)
    {
        if (!player) return false;
        Vector2 start = transform.position;
        Vector2 dir = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(start, player.position);
        if (dist > maxDist) return false;

        RaycastHit2D hit = Physics2D.Raycast(start, dir, dist, obstacleMask);
        if (hit) return false;

        return true;
    }

    // 벽 개수 세기 (감쇠용)
    public int CountObstaclesBetween(Vector2 from, Vector2 to)
    {
        Vector2 dir = (to - from).normalized;
        float dist = Vector2.Distance(from, to);
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir, dist, obstacleMask);
        return hits.Length;
    }

    // 청각 – 벽 감쇠 적용
    public bool CanHearPlayer(float baseRange)
    {
        if (!player) return false;

        float dist = Vector2.Distance(transform.position, player.position);
        int walls = CountObstaclesBetween(transform.position, player.position);

        float reduced = baseRange - walls * data.soundObstaclePenalty;
        reduced = Mathf.Max(0f, reduced);

        float hearable = reduced + (PlayerSoundRange.Instance ? PlayerSoundRange.Instance.NoiseRadius : 0f);
        return dist <= hearable;
    }
    #endregion
}