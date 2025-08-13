using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Hound/Return To Spawner")]
public class HoundReturnBehaviourSO : AttackBehaviourSO
{
    public override bool CanRun(MonsterContext ctx)
       => 
          Vector2.Distance(ctx.transform.position, ctx.spawner)
             > ctx.data.maxSpawnerDist;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.IsFastReturn = true;
        ctx.sm.ChangeState(new MonsterReturnState(ctx, ctx.sm));
        yield break;
    }
}