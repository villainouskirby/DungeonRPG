using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviours/Hound/Back Jump")]
public class HoundBackstepJumpBehaviourSO : AttackBehaviourSO
{
    public float jumpDistance = 3f;
    public float jumpDuration = 0.5f;   // 전체 점프 시간
    public float sampleRadius = 0.8f;   // 스냅 반경

    public override bool CanRun(MonsterContext ctx)
        => ctx.player != null;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        if (!ctx.player) yield break;

        ctx.animationHub?.SetTag(MonsterStateTag.BackStep, ctx);

        Vector3 start = ctx.transform.position;
        Vector3 dirToPlayer = (ctx.player.position - start).normalized;
        Vector3 backDir = -dirToPlayer;
        Vector3 target = start + backDir * jumpDistance;

        // 최종 타겟을 한 번 스냅(경계 밖이면 살짝 안쪽으로)
        if (!NavMesh.SamplePosition(target, out var hitTarget, sampleRadius, NavMesh.AllAreas))
        {
            // 타겟 스냅도 실패 → 해당 행동 스킵 (근접 or 오빗)
            ctx.nextBehaviourIndex = (Random.Range(0, 100) < 50) ? 0 : 2;
            yield break;
        }
        target = hitTarget.position;

        ctx.agent.isStopped = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);

            // Lerp로 수동 이동 (Charge 방식)
            Vector3 want = Vector3.Lerp(start, target, t);

            if (NavMesh.SamplePosition(want, out var snap, sampleRadius, NavMesh.AllAreas))
                ctx.transform.position = snap.position;
            else
            {
                // 중간 스냅 실패 시, 바로 루프 탈출 → 다음 분기로 넘김
                break;
            }

            if (ctx.player)
            {
                Vector2 toPlayerNow = (ctx.player.position - ctx.transform.position).normalized;
                ctx.SetForward(toPlayerNow);
            }

            yield return null;
        }

        ctx.agent.isStopped = false;

        // 분기(근접 50 / 오빗 50)
        ctx.nextBehaviourIndex = (Random.Range(0, 100) < 50) ? 0 : 2;

        // 점프 끝나면 플레이어를 바라보게
        if (ctx.player)
            ctx.SetForward((ctx.player.position - ctx.transform.position).normalized);
    }

    public override void OnInterrupt(MonsterContext ctx)
    {
        ctx.agent.isStopped = false;
    }
}