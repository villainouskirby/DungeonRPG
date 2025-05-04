using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어를 보면 ‘도망 + 땅속 숨기’만 수행하는 딱정벌레
/// </summary>
public class BeetleMonster : MonsterBase
{
    /* ───── 커스텀 애니메이션 이름 ───── */
    protected override string IdleAnim => "BeetleIdle";
    protected override string WalkAnim => "BeetleRun";
    protected override string RunAnim => "BeetleRun";   // 도망도 Run 재사용
    protected override string AttackAnim => "";            // 사용 안 함
    protected override string DieAnim => "BeetleBurrow"; // 땅속 숨기

    // OnTriggerStay2D에서 ‘Combat’ 대신 바로 Flee 상태로 전환 
    protected override void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player") &&
        CanSeePlayer(col.transform, data.sightDistance))
    {
        player = col.transform;
        if (state != State.Killed && state != State.Escaped)
            ChangeState(State.Flee);
    }
    }
    protected override IEnumerator Combat()
    {
        ChangeState(State.Flee);
        yield break;
    }
    // 2. Flee 코루틴 구현 (4초 도망 → 1초 burrow → 소멸) 
    protected override IEnumerator Flee()
    {
        Debug.Log("Flee 상태 시작");
        float fleeTime = 4f;
        float t = 0f;

        Play(RunAnim);
        agent.speed = data.FleeSpeed;

        while (t < fleeTime && state == State.Flee)
        {
            if (!player) { ChangeState(State.Return); yield break; }
            if (TooFarFromSpawner()) { ChangeState(State.Return); yield break; }

            // ── 도망 목적지 계산 ───────────────────────
            Vector2 away = (transform.position - player.position).normalized;
            Vector2 perp = Vector2.Perpendicular(away).normalized;      // cos ≈ 0
            // dot 계산해가며 왼쪽·오른쪽 중 더 멀리 갈 방향을 택함
            Vector2 dir =
                (Vector2.Dot(perp, player.right) > 0f) ? perp : -perp;

            float fleeDist = 4f; // 한 번에 4m 정도 이동
            Vector3 dest = transform.position + (Vector3)dir * fleeDist;
            agent.SetDestination(dest);

            // 다음 목적지를 1초 주기로 새로 잡는다
            float interval = 1f;
            float intTimer = 0f;
            while (intTimer < interval && state == State.Flee)
            {
                intTimer += Time.deltaTime;
                t += Time.deltaTime;
                yield return null;
            }
        }

        ChangeState(State.Escaped);   
    }

    /* 3. Killed 코루틴만 ‘숨기 → 파괴’ 로 오버라이드 */
    protected override IEnumerator Escaped()
    {
        Play(DieAnim);              // "BeetleBurrow"
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}