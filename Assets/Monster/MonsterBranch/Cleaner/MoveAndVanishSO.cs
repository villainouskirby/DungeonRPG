using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Cleaner/Move And Vanish")]
public class MoveAndVanishSO : SpecialBehaviourSO
{
    [Header("이동 방향·거리")]
    public float minDistance = 3f;     //  최소 이동 거리
    public float maxDistance = 6f;     //  최대 이동 거리
    [Range(0, 180)] public float angleOffset = 30f; //  반대방향에서 ±각도

    [Header("속도 옵션")]
    public float[] speedOptions = { 1f, 2f, 3f };   //  배열 중 랜덤 선택

    [Header("타이밍")]
    public float moveTime = 2f;    //  몇 초 동안 움직일지
    public float standTime = 1f;    //  이동 후 정지 시간
    public float vanishDelay = 0.5f; // 소멸 애니메이션 대기

    public override bool CanRun(MonsterContext ctx)
        => ctx.CanSeeObject(ctx.data.sightDistance) == null;   // 더 이상 아이템 없음

    public override IEnumerator Execute(MonsterContext ctx)
    {
        //  방향 계산 (아이템 반대 + 각도 오차)
        Vector2 baseDir = ctx.lastItemDir == Vector2.zero
                        ? Random.insideUnitCircle.normalized
                        : -ctx.lastItemDir;

        float deltaDeg = Random.Range(-angleOffset, angleOffset);
        Vector2 dir = Quaternion.Euler(0, 0, deltaDeg) * baseDir;

        // 거리·속도 선택
        float dist = Random.Range(minDistance, maxDistance);
        float speed = speedOptions.Length > 0
                    ? speedOptions[Random.Range(0, speedOptions.Length)]
                    : 2f;  // fallback

        Vector3 dest = ctx.transform.position + (Vector3)(dir.normalized * dist);
        ctx.agent.speed = speed;
        ctx.agent.SetDestination(dest);

        //  moveTime 동안 이동(도착하면 조기 종료)
        float t = 0f;
        while (t < moveTime)
        {
            if (!ctx.agent.isOnNavMesh) break;  // NavMesh 이탈 안전장치
            if (!ctx.agent.pathPending &&
                ctx.agent.remainingDistance <= ctx.agent.stoppingDistance) break;

            t += Time.deltaTime;
            yield return null;
        }

        // standTime 동안 제자리 대기
        ctx.agent.ResetPath();
        yield return new WaitForSeconds(standTime);

        // vanish 직전
        ctx.agent.enabled = false;   // NavMeshAgent 업데이트 완전 차단

        /*  소멸 애니메이션 */
        ctx.anim.Play("Vanish");
        yield return new WaitForSeconds(vanishDelay);

        ctx.mono.StopAllCoroutines();
        ctx.mono.enabled = false;

        /*  실제 파괴 */
        Object.Destroy(ctx.mono.gameObject);
        yield break;
    }
}