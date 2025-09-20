using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviours/Titan/Dash")]
public class TitanDashAttackSO : AttackBehaviourSO
{
    [Header("Dash params")]
    public float windup = 0.25f;     // 시전 딜레이
    public float recover = 0.35f;    // 후딜레이
    public float dashSpeed = 10f;    // 속도
    public float maxDashSeconds = 1.2f; // 안전 상한
    public int damage = 30;          // 접촉/근접 판정 데미지
    public float hitRadius = 1.0f;   // 접촉 반경

    [Header("After-branch %")]
    [Range(0, 100)] public int repeatDashPercent = 80; // 80%: 대시 또 하기

    public override bool CanRun(MonsterContext ctx)
    {
        return ctx.player != null;
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        Vector3 start = ctx.transform.position;
        Vector2 dir = (ctx.player.position - ctx.transform.position).normalized;

        // 준비
        ctx.SafeStopAgent();
        ctx.SetForward(dir);
        ctx.anim.Play("DashPrep");
        yield return new WaitForSeconds(windup);

        // 돌진
        ctx.anim.Play("Dash");
        float elapsed = 0f;
        Vector3 pos = ctx.transform.position;

        while (elapsed < maxDashSeconds)
        {
            elapsed += Time.deltaTime;
            Vector3 next = pos + (Vector3)(dir * dashSpeed * Time.deltaTime);

            // NavMesh 상 차단되면 종료
            if (NavMesh.Raycast(pos, next, out _, NavMesh.AllAreas))
                break;

            // 샘플 실패(메시 바깥)면 종료
            if (!NavMesh.SamplePosition(next, out _, 0.2f, NavMesh.AllAreas))
                break;

            // 이동
            ctx.transform.position = next;
            pos = next;

            // 히트 판정(가까우면 1회 타격)
            if (ctx.player &&
                Vector2.Distance(ctx.transform.position, ctx.player.position) <= hitRadius)
            {
                ctx.player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }

            yield return null;
        }

        // 종료 후 플레이어 바라보기
        if (ctx.player)
        {
            Vector2 toPlayer = (ctx.player.position - ctx.transform.position).normalized;
            ctx.SetForward(toPlayer);
        }

        // 회복
        ctx.anim.Play("DashRecover");
        yield return new WaitForSeconds(recover);

        // 패턴 누적 + 쉬기 강제 분기
        ctx.patternCount++;
        if (ctx.patternCount >= ctx.PatternEveryRest)
        {
            ctx.patternCount = 0;
            // Rest는 공격배열 index 2
            ctx.nextBehaviourIndex = 2;
            yield break;
        }

        // 80% 재돌진, 20% 이동(Approach)
        int atkCount = ctx.data.combatAttackBehaviours?.Length ?? 0; // =3
        int approachComboIdx = atkCount + 0; // 이동배열 0번 = Approach → 연속인덱스 3

        int roll = Random.Range(0, 100);
        ctx.nextBehaviourIndex = (roll < repeatDashPercent) ? 1 : approachComboIdx;
    }
}
