using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviours/Titan/Dash (SetDestination -> Overshoot, LockAtDashStart)")]
public class TitanDashSO : AttackBehaviourSO
{
    [Header("Distances")]
    [Tooltip("목표 지점에 도착으로 간주할 거리")]
    public float arriveThreshold = 0.15f;
    [Tooltip("플레이어를 지난 뒤 같은 방향으로 관통할 거리")]
    public float overshootDistance = 4f;   // 요구: 4f

    [Header("Timing / Speed")]
    public float windup = 1f;              // 준비 동작
    public float recover = 3f;             // 후딜
    public float dashSpeed = 12f;
    public float maxDashSeconds = 1.2f;    // 전체 안전 상한

    [Header("Hit")]
    public int damage = 30;
    public float hitRadius = 1.0f;
    public LayerMask hitMask;              // Player/Farm 등
    public bool hitTriggersOnly = false;

    [Header("NavMesh")]
    public float sampleRadius = 1.5f;      // 목적지 스냅 반경
    public float rayStep = 0.25f;          // 직선 샘플 간격
    const float REPICK_COOLDOWN = 0.1f;    // 경로 깨졌을 때 재계산 쿨다운

    [Header("After-branch %")]
    [Range(0, 100)] public int repeatDashPercent = 80;

    [Header("Reattach safety")]
    public float reattachRadius = 2f;

    readonly Collider2D[] _buf = new Collider2D[16];

    public override bool CanRun(MonsterContext ctx) => ctx.player && ctx.agent;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        if (!ctx.player) yield break;
        if (!ctx.EnsureAgentReady(sampleRadius)) yield break;

        // Agent 런 세팅
        float prevSpeed = ctx.agent.speed;
        float prevStop = ctx.agent.stoppingDistance;
        bool prevBrake = ctx.agent.autoBraking;
        var prevAvoid = ctx.agent.obstacleAvoidanceType;

        ctx.SafeStopAgent();
        ctx.agent.updateRotation = false;
        ctx.agent.updateUpAxis = false;
        ctx.agent.isStopped = false;
        ctx.agent.speed = dashSpeed;
        ctx.agent.stoppingDistance = 0f;
        ctx.agent.autoBraking = false;
        ctx.agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        // 프렙
        ctx.animationHub?.SetTag(MonsterStateTag.Change, ctx);
        yield return new WaitForSeconds(windup);

        // === ★ 대시 '시작' 시점에 플레이어 현재 위치를 캡처한다(호밍 없음) ===
        Vector3 dashStartPos = ctx.transform.position;
        Vector3 playerAtDashStart = ctx.player ? ctx.player.position : dashStartPos;
        Vector3 toP0 = playerAtDashStart - dashStartPos;
        Vector3 dir0 = (toP0.sqrMagnitude < 1e-6f) ? ctx.GetForward() : toP0.normalized;

        FaceImmediate(ctx, (Vector2)dir0);

        // ========== 1) 플레이어 위치로 돌진 ==========
        ctx.animationHub?.SetTag(MonsterStateTag.Roll, ctx);
        Vector3 goal1 = SnapReachableAlongRay(
            from: dashStartPos,
            dir: dir0,
            distance: (playerAtDashStart - dashStartPos).magnitude,
            sampleR: sampleRadius,
            step: rayStep,
            pathStartForCalc: dashStartPos
        );
        ctx.TrySetDestinationSafe(goal1, sampleRadius);

        float elapsed = 0f, repickCd = 0f;
        bool hitOnce = false;
        var farmHit = new HashSet<object>();

        while (elapsed < maxDashSeconds)
        {
            elapsed += Time.deltaTime;
            repickCd -= Time.deltaTime;

            HandleHits(ctx, ref hitOnce, farmHit);

            // 도착
            if (!ctx.agent.pathPending &&
                ctx.agent.remainingDistance <= Mathf.Max(arriveThreshold, ctx.agent.stoppingDistance))
                break;

            // '지나쳤는지' 판정: 목표점을 기준으로 앞으로 넘어갔는지 확인 (dir0 기준)
            Vector3 cur = ctx.transform.position;
            float passed = Vector3.Dot(cur - playerAtDashStart, dir0);
            if (passed > 0f) break;

            // 경로 깨짐 → 동일 직선상 최원점 재계산
            if (!ctx.agent.pathPending && ctx.agent.pathStatus != NavMeshPathStatus.PathComplete && repickCd <= 0f)
            {
                Vector3 from = ctx.transform.position;
                float remain = Mathf.Max(0f, Vector3.Dot(playerAtDashStart - from, dir0));
                if (remain > 0.05f)
                {
                    Vector3 reGoal = SnapReachableAlongRay(from, dir0, remain, sampleRadius, rayStep, dashStartPos);
                    ctx.TrySetDestinationSafe(reGoal, sampleRadius);
                }
                repickCd = REPICK_COOLDOWN;
            }

            FaceImmediate(ctx, ctx.agent.velocity.sqrMagnitude > 1e-6f ? (Vector2)ctx.agent.velocity.normalized : (Vector2)dir0);
            yield return null;
        }

        // ========== 2) 동일 방향으로 overshootDistance 더 돌진 ==========
        {
            Vector3 from = ctx.transform.position;
            Vector3 goal2 = SnapReachableAlongRay(from, dir0, overshootDistance, sampleRadius, rayStep, from);
            ctx.TrySetDestinationSafe(goal2, sampleRadius);

            while (elapsed < maxDashSeconds)
            {
                elapsed += Time.deltaTime;
                repickCd -= Time.deltaTime;

                HandleHits(ctx, ref hitOnce, farmHit);

                if (!ctx.agent.pathPending &&
                    ctx.agent.remainingDistance <= Mathf.Max(arriveThreshold, ctx.agent.stoppingDistance))
                    break;

                if (!ctx.agent.pathPending && ctx.agent.pathStatus != NavMeshPathStatus.PathComplete && repickCd <= 0f)
                {
                    Vector3 cur = ctx.transform.position;
                    float remain = Mathf.Max(0f, overshootDistance - Vector3.Dot(cur - from, dir0));
                    if (remain > 0.05f)
                    {
                        Vector3 reGoal = SnapReachableAlongRay(cur, dir0, remain, sampleRadius, rayStep, from);
                        ctx.TrySetDestinationSafe(reGoal, sampleRadius);
                    }
                    repickCd = REPICK_COOLDOWN;
                }

                FaceImmediate(ctx, ctx.agent.velocity.sqrMagnitude > 1e-6f ? (Vector2)ctx.agent.velocity.normalized : (Vector2)dir0);
                yield return null;
            }
        }

        // 안전 재부착 + 설정 복구
        if (!ctx.agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(ctx.transform.position, out var snap, reattachRadius, NavMesh.AllAreas))
                ctx.agent.Warp(snap.position);
            else
                ctx.EnsureAgentReady(reattachRadius);
        }
        ctx.SafeStopAgent();
        ctx.agent.speed = prevSpeed;
        ctx.agent.stoppingDistance = prevStop;
        ctx.agent.autoBraking = prevBrake;
        ctx.agent.obstacleAvoidanceType = prevAvoid;

        // 후딜
        ctx.animationHub?.SetTag(MonsterStateTag.ReChange, ctx);
        if (recover > 0f) yield return new WaitForSeconds(recover);

        // 패턴 분기(기존 유지)
        ctx.patternCount++;
        if (ctx.patternCount >= ctx.PatternEveryRest)
        {
            ctx.patternCount = 0;
            ctx.nextBehaviourIndex = 2; // Rest
            yield break;
        }
        int atkCount = ctx.data.combatAttackBehaviours?.Length ?? 0;
        int approachIdx = atkCount + 0; // 이동배열 0번(Approach)
        int roll = Random.Range(0, 100);
        ctx.nextBehaviourIndex = (roll < repeatDashPercent) ? 1 : approachIdx;
    }

    // 직선(ray) 상에서 0..distance 구간을 step 간격으로 훑어보며
    // NavMesh 위이고 경로가 "완전한" 가장 먼 점을 골라 반환
    Vector3 SnapReachableAlongRay(Vector3 from, Vector3 dir, float distance, float sampleR, float step, Vector3 pathStartForCalc)
    {
        if (!NavMesh.SamplePosition(pathStartForCalc, out var sHit, sampleR, NavMesh.AllAreas))
            sHit.position = pathStartForCalc;

        Vector3 best = from;
        bool found = false;

        float maxD = Mathf.Max(0.01f, distance);
        for (float d = step; d <= maxD + 1e-3f; d += step)
        {
            Vector3 p = from + dir * Mathf.Min(d, maxD);
            if (!NavMesh.SamplePosition(p, out var hit, sampleR, NavMesh.AllAreas)) continue;

            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(sHit.position, hit.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete)
            {
                best = hit.position;
                found = true;
            }
        }
        return found ? best : from;
    }

    void HandleHits(MonsterContext ctx, ref bool hitPlayerOnce, HashSet<object> farmHit)
    {
        // 플레이어 1회
        if (!hitPlayerOnce && ctx.player &&
            Vector2.Distance(ctx.transform.position, ctx.player.position) <= hitRadius)
        {
            var def = PlayerData.Instance ? PlayerData.Instance.GetComponent<PlayerDefense>() : null;
            int final = damage;
            if (def) final = def.ResolveGuard(damage);
            if (PlayerData.Instance) PlayerData.Instance.HPValueChange(-final);
            hitPlayerOnce = true;
        }

        // 자원 드롭 유도
        int n = Physics2D.OverlapCircleNonAlloc(ctx.transform.position, hitRadius, _buf, hitMask);
        for (int i = 0; i < n; i++)
        {
            var col = _buf[i];
            if (!col || !col.enabled) continue;
            if (hitTriggersOnly && !col.isTrigger) continue;
            if (col.transform == ctx.transform) continue;

            if (col.CompareTag("Farm") && col.TryGetComponent(out ResourceNodeBase f))
            {
                if (farmHit.Add(f))
                    f.Damage(damage);
            }
        }
    }

    static void FaceImmediate(MonsterContext ctx, Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) return;
        ctx.anim?.SetFloat("DirX", dir.x);
        ctx.anim?.SetFloat("DirY", dir.y);
        if (ctx.sr) ctx.sr.flipX = (dir.x < 0f);
    }
}
