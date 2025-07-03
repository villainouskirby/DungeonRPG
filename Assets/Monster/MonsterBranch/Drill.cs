using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "Behaviours/Attack/Drill Attack")]
public class DrillBehaviour : AttackBehaviourSO
{
    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.anim.Play("Attack1");
        yield return new WaitForSeconds(ctx.data.attackCooldown);
        Debug.Log($"{ctx.data.monsterName} Drill");
    }
}