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
        Vector2 dir = (ctx.player.position - ctx.transform.position).normalized;
        /* 1) 준비 */
        ctx.SetForward(dir);
        ctx.agent.isStopped = true;
        ctx.agent.velocity = Vector3.zero;
        ctx.animationHub?.SetTag(MonsterStateTag.Idle, ctx);
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(windupTime / 3);
            ctx.SetForward(dir);
        }


        /* 2) 돌진 */
        ctx.anim.Play("Charge");

        ctx.SetForward(dir);
        ctx.anim.Play("Charge");

        ctx.animationHub?.SetTag(MonsterStateTag.CombatAttack, ctx);
        float travelled = 0f;
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

        // 돌진 끝난 후 플레이어 쪽으로 다시 방향 갱신
        if (ctx.player)
        {
            Vector2 toPlayer = (ctx.player.position - ctx.transform.position).normalized;
            ctx.SetForward(toPlayer);
        }

        int roll = Random.Range(0, 100);
        if (roll < 30) ctx.nextBehaviourIndex = 0;   // 근접공격
        else ctx.nextBehaviourIndex = 3;             // 백점프
    }

    public override void OnInterrupt(MonsterContext ctx)
    {
        ctx.agent.isStopped = false;   // 중단 시 이동 해제
    }
}