using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Titan/Rest")]
public class TitanRestSO : AttackBehaviourSO
{
    [Header("Rest params")]
    public float restSeconds = 1.0f; // 휴식 시간(인스펙터에서 조절)

    public override bool CanRun(MonsterContext ctx) => true; // 강제분기로 들어오므로 항상 OK

    public override IEnumerator Execute(MonsterContext ctx)
    {
        ctx.SafeStopAgent();
        ctx.anim.Play("Rest"); // 없으면 Idle로 대체해도 됨

        float t = 0f;
        while (t < restSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 쉬면 카운트 리셋
        ctx.patternCount = 0;

        // 쉬고 나면 이동(Approach)으로 넘김
        int atkCount = ctx.data.combatAttackBehaviours?.Length ?? 0; // =3
        ctx.nextBehaviourIndex = atkCount + 0; // Approach = 3
    }
}
