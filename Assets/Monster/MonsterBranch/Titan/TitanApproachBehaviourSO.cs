using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviours/Titan/Approach (2x Hop 1f w/ Delay)")]
public class TitanApproachTwoHopsSO : AttackBehaviourSO
{
    [Header("Hop settings")]
    public int hopCount = 2;               // 2번
    public float hopDistance = 1f;         // 한 번에 1f
    public float interHopDelay = 0.5f;    // ★ 두 접근 사이 간격(초)
    public float arriveThreshold = 0.2f;   // 도착 판정
    public float hopTimeout = 1.0f;        // 각 홉 최대 대기
    public float sampleRadius = 2f;        // NavMesh 샘플 반경

    [Header("After branch")]
    public float meleeDistanceOverride = -1f; // <=0이면 data.attackEnterDistance 사용

    public override bool CanRun(MonsterContext ctx) => ctx.player && ctx.agent;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        if (!ctx.player) yield break;
        if (!ctx.EnsureAgentReady(sampleRadius)) yield break;

        ctx.animationHub?.SetTag(MonsterStateTag.CombatMove, ctx);
        ctx.agent.isStopped = false;
        ctx.agent.updateRotation = false;
        ctx.agent.updateUpAxis = false;

        float meleeDist = (meleeDistanceOverride > 0f)
            ? meleeDistanceOverride
            : Mathf.Max(0.01f, ctx.data.attackEnterDistance);

        for (int i = 0; i < Mathf.Max(1, hopCount); i++)
        {
            if (!ctx.player) break;

            Vector3 me = ctx.transform.position;
            Vector3 toP = (ctx.player.position - me);
            if (toP.sqrMagnitude < 1e-6f) break;

            Vector3 dir = toP.normalized;
            ctx.SetForward(dir);

            Vector3 target = me + dir * hopDistance;

            // NavMesh 위로 스냅
            if (NavMesh.SamplePosition(target, out var snap, sampleRadius, NavMesh.AllAreas))
            {
                if (ctx.TrySetDestinationSafe(snap.position, sampleRadius))
                {
                    ctx.anim.Play("Walk");
                    float t = 0f;
                    while (t < hopTimeout)
                    {
                        t += Time.deltaTime;

                        if (!ctx.agent.pathPending &&
                            ctx.agent.remainingDistance <= Mathf.Max(arriveThreshold, ctx.agent.stoppingDistance))
                            break;

                        // 경로 깨지면 재시도
                        if (!ctx.agent.pathPending &&
                            ctx.agent.pathStatus != NavMeshPathStatus.PathComplete)
                        {
                            ctx.EnsureAgentReady(sampleRadius);
                            ctx.TrySetDestinationSafe(snap.position, sampleRadius);
                        }
                        yield return null;
                    }
                }
            }

            // ★ 홉 사이 간격 대기
            if (interHopDelay > 0f)
                yield return new WaitForSeconds(interHopDelay);
        }

        // 접근 후 분기: 근접이면 스핀(0), 아니면 대시(1)
        if (ctx.player)
        {
            float d = Vector2.Distance(ctx.transform.position, ctx.player.position);
            ctx.SetForward((ctx.player.position - ctx.transform.position).normalized);
            ctx.nextBehaviourIndex = (d <= meleeDist) ? 0 : 1;
        }
    }
}