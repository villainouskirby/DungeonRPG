using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "Behaviours/Cleaner/Consume Item")]
public class ConsumeItemSO : SpecialBehaviourSO
{
    public float eatTime = 4f;
    public float consumeDist = 1f;   // 근접 거리

    public override bool CanRun(MonsterContext ctx)
    {
        Transform t = ctx.CanSeeObject(consumeDist);
        return t && ctx.agent.remainingDistance <= consumeDist + 0.05f;
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.anim.Play("Eat");
        yield return new WaitForSeconds(eatTime);

        Transform t = ctx.CanSeeObject(consumeDist);
        if (t) Object.Destroy(t.gameObject);

        yield return null;
    }
}
