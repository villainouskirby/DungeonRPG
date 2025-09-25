using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Titan/SpinAttack")]
public class TitanSpinAttackSO : AttackBehaviourSO
{
    [Header("Spin params")]
    public float radius = 2.5f;          // 주변공격 반경 (조절)
    public int damage = 25;              // 데미지 (조절)
    public float windup = 0.35f;         // 시전 딜레이 (조절)
    public float spinTime = 0.4f;        // 회전 시간 (조절)
    public float recover = 0.3f;         // 후딜레이 (조절)

    [Header("After-branch %")]
    [Range(0, 100)] public int repeatSpinPercent = 20;  // 20%: 스핀 한 번 더

    public override bool CanRun(MonsterContext ctx)
    {
        if (!ctx.player) return false;
        float d = Vector2.Distance(ctx.transform.position, ctx.player.position);
        return d <= radius * 1.2f; // 살짝 여유
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        // 준비
        ctx.SafeStopAgent();
        ctx.animationHub?.SetTag(MonsterStateTag.PreSpinAttack, ctx);
        yield return new WaitForSeconds(windup);

        // 시전
        ctx.animationHub?.SetTag(MonsterStateTag.SpinAttack, ctx);
        float t = 0f;
        bool hitOnce = false;
        while (t < spinTime)
        {
            t += Time.deltaTime;

            // 범위 내면 1회 타격
            if (!hitOnce && ctx.player &&
                Vector2.Distance(ctx.transform.position, ctx.player.position) <= radius)
            {
                ctx.player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                hitOnce = true;
            }
            yield return null;
        }

        // 회복
        ctx.anim.Play("SpinRecover");
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
        ctx.animationHub?.SetTag(MonsterStateTag.Idle, ctx);
        // 20% 재스핀, 80% 이동(Approach)
        int atkCount = ctx.data.combatAttackBehaviours?.Length ?? 0; // = 3(Spin,Dash,Rest)
        int approachComboIdx = atkCount + 0; // 이동배열 0번 = Approach → 연속인덱스 3

        int roll = Random.Range(0, 100);
        ctx.nextBehaviourIndex = (roll < repeatSpinPercent) ? 0 : approachComboIdx;
    }
}
