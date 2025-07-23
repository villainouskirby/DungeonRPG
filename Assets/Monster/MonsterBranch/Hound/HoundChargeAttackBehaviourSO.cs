using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Hound/Charge Attack")]
public class HoundChargeAttackBehaviourSO : AttackBehaviourSO
{
    public float chargeDistance = 4f;
    public float windupTime = 0.3f;
    public float recoverTime = 0.4f;
    public float dashSpeed = 10f;
    public int damage = 20;

    public override bool CanRun(MonsterContext ctx)
        => Vector2.Distance(ctx.transform.position, ctx.player.position) <= chargeDistance;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        /* 1) 준비 */
        ctx.anim.Play("ChargePrep");
        yield return new WaitForSeconds(windupTime);

        /* 2) 돌진 */
        ctx.anim.Play("Charge");
        Vector2 dir = (ctx.player.position - ctx.transform.position).normalized;
        float travelled = 0f;
        ctx.agent.isStopped = true;               // 경로 계산 중단

        while (travelled < chargeDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            ctx.transform.position += (Vector3)(dir * step);
            travelled += step;
            yield return null;
        }

        /* 3) 히트 판정 */
        if (Vector2.Distance(ctx.transform.position, ctx.player.position) <= ctx.data.attackRange)
            ctx.player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        /* 4) 후딜레이 */
        ctx.anim.Play("ChargeRecover");
        yield return new WaitForSeconds(recoverTime);

        ctx.agent.isStopped = false;
    }

    public override void OnInterrupt(MonsterContext ctx)
    {
        ctx.agent.isStopped = false;   // 중단 시 이동 해제
    }
}