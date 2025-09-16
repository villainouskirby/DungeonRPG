using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Hound/Back Jump")]
public class HoundBackstepJumpBehaviourSO : AttackBehaviourSO
{
    public float jumpDistance = 3f;
    public float jumpDuration = 0.5f;

    public override bool CanRun(MonsterContext ctx)
        => ctx.player != null;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.anim.Play("Jump"); // 전용 애니메이션 준비

        Vector2 dirToPlayer = (ctx.player.position - ctx.transform.position).normalized;
        Vector2 backDir = -dirToPlayer;   // 뒤쪽으로 이동
        Vector3 start = ctx.transform.position;
        Vector3 target = start + (Vector3)(backDir * jumpDistance);

        float t = 0f;
        ctx.agent.isStopped = true;

        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            ctx.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        ctx.agent.isStopped = false;

        // 행동 종료 후 후행 분기 (예: 근접공격 50%, 횡이동 50%)
        int roll = Random.Range(0, 100);
        if (roll < 50) ctx.nextBehaviourIndex = 0;   // 근접공격
        else ctx.nextBehaviourIndex = 2;             // Orbit
    }

    public override void OnInterrupt(MonsterContext ctx)
    {
        ctx.agent.isStopped = false;
    }
}