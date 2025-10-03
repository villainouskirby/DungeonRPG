using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Hound/Approach Player")]
public class HoundApproachBehaviourSO : AttackBehaviourSO
{
    [Tooltip("이 거리 이내로 접근할 때까지 실행")]
    public float desiredDistance = 2f;

    public override bool CanRun(MonsterContext ctx)
        => Vector2.Distance(ctx.transform.position, ctx.player.position) > desiredDistance;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.agent.speed = ctx.data.combatSpeed;
        ctx.animationHub?.SetTag(MonsterStateTag.CombatMove, ctx);

        /* 원하는 거리까지 계속 이동 */
        while (Vector2.Distance(ctx.transform.position, ctx.player.position) > desiredDistance)
        {
            ctx.agent.SetDestination(ctx.player.position);
            yield return null;
        }
        int roll = Random.Range(0, 100);
        if (roll < 30) ctx.nextBehaviourIndex = 0;   // 근접공격
        else ctx.nextBehaviourIndex = 2;             // Orbit
    }
}
