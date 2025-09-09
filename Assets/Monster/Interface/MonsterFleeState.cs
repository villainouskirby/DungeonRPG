using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public sealed class MonsterFleeState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;
    Coroutine running;

    public MonsterFleeState(MonsterContext c, MonsterStateMachine r)
    { ctx = c; root = r; }

    public void Enter()
    {
        running = ctx.mono.StartCoroutine(FleeRoutine());
    }

    public void Exit()
    {
        if (running != null) ctx.mono.StopCoroutine(running);
        ctx.agent.ResetPath();
    }

    public void Tick() { }   // 로직은 코루틴 안에서

    IEnumerator FleeRoutine()
    {
        const float fleeDuration = 4f;
        const float resampleCycle = 1f;
        const float dirSampleDist = 4f;
        const float stuckCheckTime = 0.25f;
        const float stuckMoveEps = 0.05f;

        ctx.anim.Play("Run");
        ctx.agent.speed = ctx.data.fleeSpeed;

        float elapsed = 0f;

        while (elapsed < fleeDuration)
        {
            // 반대 + X성분 우선 방향 찾기
            Vector2 away = (ctx.transform.position - ctx.player.position).normalized;

            Vector2 bestDir = away;
            float bestDot = -1f;
            const int samples = 16;

            for (int i = 0; i < samples; ++i)
            {
                float angle = i * (360f / samples) * Mathf.Deg2Rad;
                Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));

                if (Vector2.Dot(dir, away) <= 0f) continue;   // 플레이어 반대 방향만

                float score = Mathf.Abs(dir.x);               // X 절대값이 클수록 우선
                if (score > bestDot) { bestDot = score; bestDir = dir; }
            }

            Vector3 dest = ctx.transform.position + (Vector3)(bestDir * dirSampleDist);
            ctx.agent.SetDestination(dest);

            // 이동했는지 확인
            Vector3 prevPos = ctx.transform.position;
            float stuckT = 0f;
            bool moved = false;

            while (stuckT < stuckCheckTime)
            {
                stuckT += Time.deltaTime;
                elapsed += Time.deltaTime;

                if ((ctx.transform.position - prevPos).sqrMagnitude >
                    stuckMoveEps * stuckMoveEps)
                { moved = true; break; }

                yield return null;
            }

            // 막혔으면 X축 반대방향으로 한 번 더
            if (!moved)
            {
                Vector2 oppositeDir = -bestDir.normalized;
                float runT = 0f;

                while (runT < 3f)                     // 3초 동안
                {
                    runT += Time.deltaTime;
                    elapsed += Time.deltaTime;

                    // 매 프레임 앞으로 dirSampleDist 만큼 목표 갱신
                    Vector3 des = ctx.transform.position + (Vector3)(oppositeDir * dirSampleDist);
                    ctx.agent.SetDestination(des);

                    yield return null;
                }
                continue;   // 3초 후 다시 샘플링 루프로
            }

            // 정상 이동 중이면 1초 후 재계산
            yield return new WaitForSeconds(resampleCycle);
            elapsed += resampleCycle;
        }

        root.ChangeState(new MonsterIdleState(ctx, root));   // 필요하면 Idle/Return로 교체
    }
}