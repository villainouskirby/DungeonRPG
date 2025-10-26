using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "Behaviours/Cleaner/Move To Item")]
public class MoveToItemSO : SpecialBehaviourSO
{
    public float searchRadius = 8f;   // 탐지 범위
    public float stopDist = 0.5f; // 도착 판정
    public float moveSpeed = 2f;

    public override bool CanRun(MonsterContext ctx)
    {
        Transform t = ctx.CanSeeObject(searchRadius);
        return t && Vector2.Distance(ctx.transform.position, t.position) > stopDist;
    }

    public override IEnumerator Execute(MonsterContext ctx)
    {
        SoundManager.Instance.PlaySound3D(
                "SFX_CleanerWalk",
                ctx.transform,
                delay: 0f,
                isLoop: true,
                type: SoundType.SFX,
                attachToTarget: true,
                minDistance: 0f,
                maxDistance: 30f
            );
        Transform target = ctx.CanSeeObject(searchRadius);
        if (!target) yield break;

        ctx.agent.speed = moveSpeed;
        ctx.agent.SetDestination(target.position);

        // 도착·아이템소멸 둘 중 하나 까지 대기
        while (target && Vector2.Distance(ctx.transform.position, target.position) > stopDist)
            yield return null;

        SoundManager.Instance.StopLoopSound("SFX_CleanerWalk");
        ctx.SafeStopAgent();
        yield return null;

        if (target)                      // 이동 성공 시 방향 기록
            ctx.lastItemDir = (target.position - ctx.transform.position).normalized;
    }
}