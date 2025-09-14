using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Hound/Orbit Player")]
public class HoundOrbitBehaviourSO : AttackBehaviourSO
{
    [Range(10, 90)] public float angleRange = 30f;   // ±각도
    public float orbitRadius = 2.5f;                 // 플레이어와 간격
    public float orbitTime = 2f;                   // 얼마 동안 배회할지
    public float stepInterval = 0.4f;                // 목표점 변경 주기

    public override bool CanRun(MonsterContext ctx)
    {
        float d = Vector2.Distance(ctx.transform.position, ctx.player.position);
        return d <= orbitRadius * 1.3f;              // 너무 멀면 사용 안 함
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.agent.speed = ctx.data.detectSpeed;
        ctx.anim.Play("Walk");

        float t = 0f;
        while (t < orbitTime)
        {
            t += stepInterval;

            /* ±angleRange 사이 임의 각도로 목표점 계산 */
            Vector2 toPlayer = ctx.transform.position - ctx.player.position;
            float baseDeg = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float deltaDeg = Random.Range(-angleRange, angleRange);
            float targetDeg = baseDeg + deltaDeg;

            Vector2 offset = new Vector2(
                Mathf.Cos(targetDeg * Mathf.Deg2Rad),
                Mathf.Sin(targetDeg * Mathf.Deg2Rad)) * orbitRadius;

            ctx.agent.SetDestination(ctx.player.position + (Vector3)offset);

            yield return new WaitForSeconds(stepInterval);
        }
        ctx.nextBehaviourIndex = 0;   // 근접공격
    }
}