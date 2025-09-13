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
        ctx.cleaneranimPlayer?.SetTag(MonsterStateTag.Idle, ctx);
        yield return new WaitForSeconds(eatTime);

        Transform t = ctx.CanSeeObject(consumeDist);
        if (!t) yield break;

        // DropItem이면 데이터/수량을 stomach에 Push 후 풀 반환
        if (t.TryGetComponent(out DropItem di))
        {
            if (ctx.mono.TryGetComponent(out MonsterStomach stomach))
            {
                stomach.Swallow(di.Data, di.Amount);
            }
            DropItemPool.Instance.Return(di);   // 월드에서 제거(먹었음)
        }
        else
        {
            // DropItem이 아니면 그냥 파괴
            Object.Destroy(t.gameObject);
        }

        yield return null;
    }
}
