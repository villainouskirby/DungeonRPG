using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Flee/BeetleRoll")]
public class BeetleRollAway : FleeBehaviourSO
{
    public float rollSpeed = 6f;
    public float rollTime = 1.2f;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.anim.Play("Roll");
        float t = 0f;
        while (t < rollTime)
        {
            t += Time.deltaTime;
            ctx.agent.velocity = -(ctx.player.position - ctx.transform.position).normalized * rollSpeed;
            yield return null;
        }
        ctx.agent.velocity = Vector3.zero;
    }
}