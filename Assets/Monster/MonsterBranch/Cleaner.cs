using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Special/Clean")]
public class CleanBehaviour : SpecialBehaviourSO
{
    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.anim.Play("Clean");
        yield return new WaitForSeconds(ctx.data.attackCooldown);
        Debug.Log($"{ctx.data.monsterName} Clean");
    }
}