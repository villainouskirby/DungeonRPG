using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Attack/Hound Bite")]
public class HoundBiteBehaviour : AttackBehaviourSO
{
    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.anim.Play("Attack1");
        yield return new WaitForSeconds(ctx.data.attackCooldown);
        Debug.Log($"{ctx.data.monsterName} 물어뜯기 Hit!");
    }
}