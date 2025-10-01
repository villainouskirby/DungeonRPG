using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviours/Hound/Orbit Player")]
public class HoundOrbitBehaviourSO : AttackBehaviourSO
{
    [Range(10, 90)] public float angleRange = 30f;   // ±각도
    public float orbitRadius = 2.5f;                 // 플레이어와 간격
    public float orbitTime = 2f;                   // 얼마 동안 배회할지
    public float stepInterval = 0.4f;                // 목표점 변경 주기
    public float moveSpeed = 3.5f;                   // 실제 이동 속도 (수동)
    public float sampleRadius = 0.8f;
    public override bool CanRun(MonsterContext ctx)
    {
        float d = Vector2.Distance(ctx.transform.position, ctx.player.position);
        return d <= orbitRadius * 1.3f;              // 너무 멀면 사용 안 함
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.agent.speed = ctx.data.detectSpeed;
        ctx.anim.Play("Walk");
        ctx.agent.isStopped = true;

        float elapsed = 0f;
        float tick = 0f;

        while (elapsed < orbitTime)
        {
            if (!ctx.player) break;

            // 주기마다 새로운 오빗 목표 갱신
            if (tick <= 0f)
            {
                Vector2 toPlayer = ctx.transform.position - ctx.player.position;
                float baseDeg = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
                float deltaDeg = Random.Range(-angleRange, angleRange);
                float targetDeg = baseDeg + deltaDeg;

                Vector3 want = ctx.player.position + new Vector3(
                    Mathf.Cos(targetDeg * Mathf.Deg2Rad),
                    Mathf.Sin(targetDeg * Mathf.Deg2Rad)) * orbitRadius;

                // 목표 지점도 한 번 스냅(경계 바깥 방향 방지)
                if (NavMesh.SamplePosition(want, out var hit, sampleRadius, NavMesh.AllAreas))
                    _currentTarget = hit.position;
                else
                {
                    // 목표 스냅 실패 → 바로 근접공격 시도
                    ctx.agent.isStopped = false;
                    ctx.nextBehaviourIndex = 0;
                    yield break;
                }

                tick = stepInterval;
            }

            // 프레임 단위로 수동 이동 (Charge 방식)
            Vector3 cur = ctx.transform.position;
            Vector3 movedir = (_currentTarget - cur);
            float dist = movedir.magnitude;

            if (dist > 1e-4f)
            {
                Vector3 step = movedir.normalized * (moveSpeed * Time.deltaTime);
                if (step.sqrMagnitude > movedir.sqrMagnitude) step = movedir; // 오버슈트 방지

                Vector3 next = cur + step;

                // 네브메시 스냅(경계 이탈 방지)
                if (NavMesh.SamplePosition(next, out var snap, sampleRadius, NavMesh.AllAreas))
                    ctx.transform.position = snap.position;
                else
                {
                    // 스냅 실패 → 이 목표는 포기하고 다음 타겟 뽑기
                    tick = 0f;
                }

                // 시선 갱신
                ctx.SetForward((_currentTarget - ctx.transform.position).normalized);
            }

            elapsed += Time.deltaTime;
            tick -= Time.deltaTime;
            yield return null;
        }

        ctx.agent.isStopped = false;


        ctx.nextBehaviourIndex = 0;   // 근접공격

        Vector2 dir = (ctx.player.position - ctx.transform.position).normalized;
        ctx.SetForward(dir);
    }
    Vector3 _currentTarget;
}