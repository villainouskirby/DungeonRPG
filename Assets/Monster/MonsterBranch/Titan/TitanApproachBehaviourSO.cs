using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Titan/Approach")]
public class TitanApproachSO : AttackBehaviourSO
{
    [Header("Approach params")]
    public float desiredDistance = 2.2f; // 근접 거리(인스펙터 조절)
    public float moveSpeed = 3.5f;

    [Range(0, 100)] public int dashPercent = 50;

    public override bool CanRun(MonsterContext ctx)
    {
        if (!ctx.player) return false;
        return Vector2.Distance(ctx.transform.position, ctx.player.position) > desiredDistance;
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        if (!ctx.player) yield break;
        
        if (Random.Range(0, 100) < dashPercent)
        {
            ctx.nextBehaviourIndex = 1; // Dash
            yield break;
        }
        ctx.agent.speed = moveSpeed;
        ctx.anim.Play("Walk");

        while (ctx.player &&
               Vector2.Distance(ctx.transform.position, ctx.player.position) > desiredDistance)
        {
            ctx.TrySetDestinationSafe(ctx.player.position);
            yield return null;
        }
    }
}
