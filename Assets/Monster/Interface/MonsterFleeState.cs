using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterFleeState : IMonsterState
{
    enum Phase { Run, CornerDash, VanishPrep, Done }

    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    // 타이밍
    float totalElapsed;           // 총 도망 시간
    float resampleTimer;          // 재샘플 주기
    float stuckTimer;             // 막힘 체크
    float keepTimer;              // 같은 타겟 유지 시간
    float dashRemain;             // 구석 탈출 대쉬 남은 시간
    float vanishTimer;            // 사라지기 전 잠깐 멈춤 타이머

    // 위치/방향
    Vector3 lastPos;              // 막힘 감지용
    Vector3 lastMoveDir;          // 관성(방향 유지)용
    Vector3 currentTarget;

    // 링 샘플 상태
    float radius;                 // 플레이어 중심 링 반지름
    float angleBias;              // 각도 시프트
    Phase phase;
    bool cleanerMode;
    float cleanerWaitTimer;
    // MonsterData 기반 파라미터 (없으면 기본값 사용)
    float fleeDuration => ctx.data.fleeDespawnSeconds > 0 ? ctx.data.fleeDespawnSeconds : 5f;
    float resampleCycle => ctx.data.fleeResampleCycle > 0 ? ctx.data.fleeResampleCycle : 0.75f;
    float stuckWindow => ctx.data.fleeStuckCheckTime > 0 ? ctx.data.fleeStuckCheckTime : 0.30f;
    float stuckMoveEps => ctx.data.fleeStuckMoveEps > 0 ? ctx.data.fleeStuckMoveEps : 0.05f;
    float samplePosRadius => ctx.data.fleeSamplePositionRadius > 0 ? ctx.data.fleeSamplePositionRadius : 2f;

    float baseRadius => ctx.data.fleeTargetRadius > 0 ? ctx.data.fleeTargetRadius : 6f;
    float ringStep => ctx.data.fleeRingStep > 0 ? ctx.data.fleeRingStep : 1.5f;
    int angularSamples => ctx.data.fleeAngularSamples > 0 ? ctx.data.fleeAngularSamples : 16;
    int ringCount => ctx.data.fleeMultiRingCount > 0 ? ctx.data.fleeMultiRingCount : 3;

    float keepTargetTime => ctx.data.fleeKeepTargetTime > 0 ? ctx.data.fleeKeepTargetTime : 0.8f;
    float keepTargetMinMove => ctx.data.fleeKeepTargetDist > 0 ? ctx.data.fleeKeepTargetDist : 0.5f;

    float dashSeconds => ctx.data.fleeCornerDashSeconds > 0 ? ctx.data.fleeCornerDashSeconds : 2f;
    float dashStep => ctx.data.fleeCornerDashStep > 0 ? ctx.data.fleeCornerDashStep : 3f;
    float panicDistance => ctx.data.fleePanicDistance > 0 ? ctx.data.fleePanicDistance : 2.5f;

    // “플레이어로부터 멀어짐”을 더 강하게
    float W_DIST => ctx.data.fleeW_Dist != 0 ? ctx.data.fleeW_Dist : 1.2f;
    float W_LEN => ctx.data.fleeW_Len != 0 ? ctx.data.fleeW_Len : -0.20f;
    float W_AWAY => ctx.data.fleeW_Away != 0 ? ctx.data.fleeW_Away : 0.35f;
    float W_LOS => ctx.data.fleeW_LOS != 0 ? ctx.data.fleeW_LOS : 0.20f;
    float W_INERTIA => ctx.data.fleeW_Inertia != 0 ? ctx.data.fleeW_Inertia : 0.30f;

    // 바깥으로 서서히 밀어내기(플레이어가 가만히 있어도 계속 멀어짐)
    float outwardGrowPerSec => ctx.data.fleeOutwardGrowPerSec > 0 ? ctx.data.fleeOutwardGrowPerSec : (0.75f * ringStep);

    // 사라지기 전 잠깐 멈추는 시간
    float vanishPauseSeconds => ctx.data.fleeVanishPauseSeconds > 0 ? ctx.data.fleeVanishPauseSeconds : 1f;

    public MonsterFleeState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Flee);
        ctx.animationHub?.SetTag(MonsterStateTag.Flee, ctx);
        ctx.EnsureAgentReady();
        ctx.agent.isStopped = false;
        ctx.agent.autoRepath = true;
        ctx.agent.autoBraking = false;      // 급감속에 의한 와리가리 억제
        ctx.agent.speed = ctx.data.fleeSpeed;
        ctx.agent.stoppingDistance = ctx.data.stoppingDistance;
        ctx.anim.Play("Run");

        cleanerMode = (ctx.data.category == MonsterData.MonsterCategory.Cleaner);
        if (cleanerMode)
        {
            cleanerWaitTimer = 0f;
            ctx.TrySetDestinationSafe(ctx.spawner, 3f);
            return;                                    // 일반 도망 로직 생략
        }

        totalElapsed = resampleTimer = stuckTimer = keepTimer = 0f;
        dashRemain = vanishTimer = 0f;
        lastPos = ctx.transform.position;
        lastMoveDir = Vector3.zero;
        phase = Phase.Run;

        // 초기 링 반지름 – 현재 거리 + step (플레이어가 멈춰도 바깥으로 시작)
        Vector3 p = ctx.player ? ctx.player.position : ctx.transform.position;
        float cur = Vector3.Distance(ctx.transform.position, p);
        radius = Mathf.Max(baseRadius, cur + ringStep);
        angleBias = Random.Range(0f, 360f);

        RecomputeTarget(forceExpand: false);
    }

    public void Exit()
    {
        ctx.agent.ResetPath();
        ctx.agent.isStopped = false;
        ctx.agent.autoBraking = true;
    }

    public void Tick()
    {
        if (cleanerMode)
        {
            // 스포너로 계속 목적지 업데이트
            ctx.TrySetDestinationSafe(ctx.spawner, 3f);

            float dist = Vector2.Distance(ctx.transform.position, ctx.spawner);
            if (dist <= ctx.data.nearSpawnerDist + 0.05f)
            {
                // 도착 후 대기
                ctx.SafeStopAgent();
                ctx.anim.Play("Idle");
                cleanerWaitTimer += Time.deltaTime;
                if (cleanerWaitTimer >= 1f)
                {
                    // 여기서 사라지는 애니 후 지연을 추가하면 될듯
                    SpawnerPool.Instance.MonsterPool.Release(ctx.id, ctx.mono.gameObject);
                    return;
                }
            }
            return;  // Cleaner 이외에는 일반 도망 로직 건드리지 않음
        }
        // 항상 최신 플레이어 위치 사용
        Vector3 p = ctx.player ? ctx.player.position : ctx.transform.position;

        // 총 도망 시간 종료 → Vanish 준비로 전환(바로 Release 하지 않음)
        totalElapsed += Time.deltaTime;
        if (phase != Phase.VanishPrep && totalElapsed >= fleeDuration)
        {
            phase = Phase.VanishPrep;
            ctx.SafeStopAgent();
            // 사라지는 애니가 있으면 재생
            if (ctx.anim) ctx.anim.Play("Vanish", 0, 0f);
            vanishTimer = 0f;
        }

        // 페이즈 처리
        switch (phase)
        {
            case Phase.Run: TickRun(p); break;
            case Phase.CornerDash: TickDash(); break;
            case Phase.VanishPrep: TickVanishPrep(); break;
            case Phase.Done: break;
        }
    }

    void TickRun(Vector3 playerPos)
    {
        // 링 반지름을 시간에 따라 조금씩 키워서(플레이어가 멈춰도) 계속 멀어지게
        radius += outwardGrowPerSec * Time.deltaTime;

        resampleTimer += Time.deltaTime;
        keepTimer += Time.deltaTime;

        // 이동량 체크(막힘 감지)
        bool moved = (ctx.transform.position - lastPos).sqrMagnitude > stuckMoveEps * stuckMoveEps;
        if (moved)
        {
            lastMoveDir = (ctx.transform.position - lastPos).normalized;
            stuckTimer = 0f;
            lastPos = ctx.transform.position;
        }
        else
        {
            stuckTimer += Time.deltaTime;
        }

        bool pathBlocked = !ctx.agent.pathPending && ctx.agent.pathStatus != NavMeshPathStatus.PathComplete;
        bool arrived = !ctx.agent.pathPending && ctx.agent.remainingDistance <= ctx.agent.stoppingDistance + 0.05f;

        // 히스테리시스: 일정 시간이 지나면(keepTargetTime) 무조건 재샘플해서 다음 표적을 잡음
        bool keepTooLong = keepTimer >= keepTargetTime;

        // 너무 가까우면 즉시 재샘플 + 급확장
        bool playerTooClose = false;
        float dp = Vector3.Distance(ctx.transform.position, playerPos);
        if (dp <= panicDistance)
        {
            playerTooClose = true;
            radius = Mathf.Max(radius, dp + ringStep * 2f);
        }

        if (pathBlocked || stuckTimer >= stuckWindow || arrived || keepTooLong || resampleTimer >= resampleCycle || playerTooClose)
        {
            if (pathBlocked || stuckTimer >= stuckWindow)
            {
                // 구석 탈출: 반대/바깥 방향으로 2초 대쉬
                Vector3 away = (ctx.transform.position - playerPos);
                Vector3 dashDir = (lastMoveDir.sqrMagnitude > 1e-6f ? -lastMoveDir :
                                  (away.sqrMagnitude > 1e-6f ? away.normalized : Vector3.right));
                lastMoveDir = dashDir;
                dashRemain = dashSeconds;
                phase = Phase.CornerDash;
                return;
            }

            RecomputeTarget(forceExpand: arrived || playerTooClose);
        }
    }

    void TickDash()
    {
        dashRemain -= Time.deltaTime;

        // 매 프레임 앞으로 한 스텝 목적지 갱신
        var stepTarget = ctx.transform.position + (lastMoveDir.sqrMagnitude > 1e-6f ? lastMoveDir : Vector3.right) * dashStep;
        ctx.TrySetDestinationSafe(stepTarget, 3f);

        if (dashRemain <= 0f)
        {
            // 대쉬 종료 → 바로 새 표적 산정
            RecomputeTarget(forceExpand: true);
            phase = Phase.Run;
        }
    }

    void TickVanishPrep()
    {
        vanishTimer += Time.deltaTime;
        if (vanishTimer >= vanishPauseSeconds)
        {
            // 실제 디스폰
            phase = Phase.Done;
            SpawnerPool.Instance.MonsterPool.Release(ctx.id, ctx.mono.gameObject);
        }
    }

    //  샘플링
    void RecomputeTarget(bool forceExpand)
    {
        if (forceExpand) { radius += ringStep; angleBias += 37f; }

        Vector3 me = ctx.transform.position;
        Vector3 p = ctx.player ? ctx.player.position : ctx.transform.position;

        Vector3 best = me;
        float bestScore = float.NegativeInfinity;

        Vector3 away = (me - p);
        away = (away.sqrMagnitude > 1e-6f) ? away.normalized : Vector3.right;

        for (int ri = 0; ri < ringCount; ++ri)
        {
            float r = radius + ringStep * ri;

            for (int i = 0; i < angularSamples; ++i)
            {
                // away(플레이어 반대) 쪽을 중심으로 각도 스윕
                float baseDeg = Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg;
                float ang = baseDeg + angleBias + (360f * i / angularSamples);
                Vector3 dir = new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad), 0f);
                Vector3 ring = p + dir * r;

                if (!NavMesh.SamplePosition(ring, out var hit, samplePosRadius, NavMesh.AllAreas))
                    continue;

                var len = ctx.PathLengthTo(hit.position, out var st);
                if (st == NavMeshPathStatus.PathInvalid) continue;

                float distToPlayer = Vector3.Distance(hit.position, p);                   // 멀수록 좋음
                float awayDot = Vector3.Dot((hit.position - p).normalized, away);   // 반대방향성
                bool losBlocked = NavMesh.Raycast(p, hit.position, out _, NavMesh.AllAreas);

                Vector3 toCandidate = (hit.position - me);
                float inertiaDot = (lastMoveDir.sqrMagnitude > 1e-6f)
                                      ? Vector3.Dot(toCandidate.normalized, lastMoveDir) : 0f;

                float score = 0f;
                score += W_DIST * distToPlayer;
                score += W_LEN * len;                           // 음수 → 짧을수록 가점
                score += W_AWAY * awayDot;
                score += W_LOS * (losBlocked ? 1f : 0f);
                score += W_INERTIA * inertiaDot;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = hit.position;
                }
            }
        }

        if (bestScore == float.NegativeInfinity) // 후보 실패 시 단순 반대 한 스텝
            best = me + away * ringStep;

        // 충분히 달라질 때만 목적지 갱신 (소소한 떨림 방지)
        if ((best - currentTarget).sqrMagnitude > 0.25f * 0.25f)
        {
            ctx.TrySetDestinationSafe(best, 3f);
            currentTarget = best;
            keepTimer = 0f;
        }

        resampleTimer = 0f;
        stuckTimer = 0f;
        angleBias += 61f;
    }
}
