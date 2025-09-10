using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterContext
{
    public readonly Monster_Info_Monster statData;
    public readonly MonsterData data;
    public readonly Transform transform;
    public readonly NavMeshAgent agent;
    public readonly Animator anim;
    public readonly SpriteRenderer sr;
    //public readonly Vector3 spawner;
    public Vector3 spawner => mono.Spawner;
    public readonly SpriteRenderer alert;
    public readonly Transform player;
    public readonly LayerMask obstacleMask;
    public readonly MonsterController mono;
    public readonly MonsterStateMachine sm;
    public readonly string[] interestTags;
    public bool isaggressive;
    public int rank;
    public float hp;
    public float attack;
    public float hearRange;
    public float sightDistance;
    public float speed;
    public Vector3 LastHeardPos;
    public bool IsFastReturn;
    public string id;
    public int hitSerial; // 피격 변화 감지용 카운터
    Vector2 _lastForward = Vector2.right;

    public MonsterContext(MonsterController owner, Monster_Info_Monster mdata)
    {
        statData = mdata;
        mono = owner;
        sm = owner.StateMachine;
        data = owner.Data;
        transform = owner.transform;
        agent = owner.Agent;
        anim = owner.Animator;
        sr = owner.Sprite;
        id = owner.monster_Id;
        //spawner = owner.Spawner;
        alert = owner.AlertSR;
        player = owner.Player;

        rank = mdata.Monster_rank;
        attack = mdata.Monster_atk;
        hp = mdata.Monster_hp;
        hearRange = mdata.Monster_sound_detection;
        sightDistance = mdata.Monster_view_detection;
        speed = mdata.Monster_speed;

        isaggressive = data.isaggressive;
        interestTags = data.interestTags;
        obstacleMask = owner.ObstacleMask;
    }

    #region 몬스터 시야 / 청각 탐지 로직
    // 시야 확인 (벽 Raycast 포함)
    public bool CanSeePlayer(float maxDist, float fovAngleDeg)
    {
        if (!player) return false;

        Vector2 start = transform.position;
        Vector2 dir = (player.position - transform.position);
        float dist = dir.magnitude;
        if (dist > maxDist) return false;

        // “몬스터가 바라보는 정면” 벡터 (2D라면 right, 3D라면 forward 등)
        Vector2 forward = GetForward();

        float halfAngle = fovAngleDeg * 0.5f;
        // 벡터 간 각도를 구해 부채꼴 범위인지 확인
        if (Vector2.Angle(forward, dir) > halfAngle) return false;

        RaycastHit2D hit = Physics2D.Raycast(start, dir.normalized, dist, obstacleMask);
        if (hit) return false;   // 벽에 가렸으면 못 봄

        return true;
    }
    public Vector2 GetForward()
    {
        // 움직이고 있으면 속도 벡터
        if (agent.velocity.sqrMagnitude > 0.001f)
            _lastForward = agent.velocity.normalized;

        // sprite FlipX 만으로 방향을 바꾼다면 ↓ 예시처럼 교체
        // _lastForward = sr.flipX ? Vector2.left : Vector2.right;

        return _lastForward;
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

    public Vector2 lastItemDir; // 아이템으로 이동했던 방향, 먹고 반대방향으로 도망가기 위한
    public Transform CanSeeObject(float maxDist)
    {
        if (interestTags == null || interestTags.Length == 0) return null;

        Vector2 start = transform.position;

        foreach (string tag in interestTags)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (obj == null) continue;                  // ← Destroy 예약된 객체는 null 로 평가된다!

                Vector2 pos = obj.transform.position;
                float dist = Vector2.Distance(start, pos);
                if (dist > maxDist) continue;
                // 현재는 그냥 거리에 따라 되는걸로 되어있음 추후에 시야각에 맞춰서 오브젝트 보도록 할 수 있을듯

                Vector2 dir = (pos - start).normalized;
                if (Physics2D.Raycast(start, dir, dist, obstacleMask)) continue;

                return obj.transform;                       // 시야 확보
            }
        }
        return null;
    }
    // 던진 오브젝트 소리 감지 (Detect 범위 내) 1번째 최우선순위
    public bool CanHearThrowObject(float detectRange, out Vector3 noisePos)
    {
        noisePos = default;
        if (!ThrowNoiseManager.Instance) return false;

        bool ok = ThrowNoiseManager.Instance.TryGetNearestNoise(transform.position, detectRange, out noisePos);
        if (ok) LastHeardPos = noisePos; // 추후 디버그/전환 로직에 활용 가능
        return ok;
    }
    #endregion
    #region 네브메쉬 안전로직
    public bool EnsureAgentReady(float sampleRadius = 3f)
    {
        if (!agent) return false;

        if (!agent.enabled) agent.enabled = true;           // 1) enable
        if (agent.isOnNavMesh) return true;                 // 2) 이미 NavMesh 위면 OK

        // 3) 현재 위치 근처에서 NavMesh 포인트를 찾아서 워프
        if (NavMesh.SamplePosition(transform.position, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            // Warp는 enabled=true여야 함
            return agent.Warp(hit.position);
        }
        return false; // 복구 실패 – SetDestination/Resume 금지
    }

    public void SafeStopAgent()
    {
        if (!agent) return;
        if (!agent.isActiveAndEnabled) return;
        if (!agent.isOnNavMesh) { EnsureAgentReady(); if (!agent.isOnNavMesh) return; }
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    public void SafeResumeAgent()
    {
        if (!agent) return;
        if (!agent.isActiveAndEnabled) return;
        if (!agent.isOnNavMesh) { EnsureAgentReady(); if (!agent.isOnNavMesh) return; }
        agent.isStopped = false; // (= Resume)
    }

    public bool TrySetDestinationSafe(Vector3 pos, float sampleRadius = 3f)
    {
        if (!EnsureAgentReady(sampleRadius)) return false;
        return agent.SetDestination(pos);
    }
    // 거리 계산
    #endregion
}