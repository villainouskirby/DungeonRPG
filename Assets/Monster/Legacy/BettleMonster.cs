using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어를 보면 ‘도망 + 땅속 숨기’만 수행하는 딱정벌레
/// </summary>
public class BeetleMonster : MonsterBase1
{
    /* ───── 커스텀 애니메이션 이름 ───── */
    protected override string IdleAnim => "BeetleIdle";
    protected override string WalkAnim => "BeetleRun";
    protected override string RunAnim => "BeetleRun";   // 도망도 Run 재사용
    protected override string AttackAnim => "";            // 사용 안 함
    protected override string DieAnim => "BeetleBurrow"; // 땅속 숨기

    //Combat’ 대신 바로 Flee 상태로 전환 
    protected override IEnumerator Combat()
    {
        ChangeState(State.Flee);
        yield break;
    }

    public override void TakeDamage(float dmg)
    {
        ChangeState(State.Flee);
        hp -= dmg;
        //HealthBarManager.Instance?.UpdateBar(this, GetHPRatio());
        Debug.Log("현재 몬스터 체력" + hp);
        if (hp <= 0 && state != State.Killed) ChangeState(State.Killed);
    }
    // Flee 코루틴 (4초 도망 → 1초 burrow → 소멸) 
    protected override IEnumerator Flee()
    {
        const float fleeDuration = 4f;   // 총 도주 시간
        const float resampleCycle = 1f;   // 목적지 재계산 주기
        const float dirSampleDist = 4f;   // 1회 이동 거리
        const float stuckCheckTime = 0.25f;
        const float stuckMoveEps = 0.05f;

        Play(RunAnim);
        agent.speed = data.fleeSpeed;

        float t = 0f;
        while (t < fleeDuration && state == State.Flee)
        {
            /* ① 플레이어 반대 + x 성분 큰 방향 찾기 */
            Vector2 away = (transform.position - playertrans.position).normalized;
            Vector2 bestDir = away;
            float bestDot = -1f;

            const int samples = 16;
            for (int i = 0; i < samples; ++i)
            {
                float angle = 360f / samples * i * Mathf.Deg2Rad;
                Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));

                if (Vector2.Dot(dir, away) <= 0f) continue;      // 반대쪽이 아니면 skip
                float score = Mathf.Abs(dir.x);                  // x 절대값 우선
                if (score > bestDot) { bestDot = score; bestDir = dir; }
            }

            agent.SetDestination(transform.position + (Vector3)(bestDir * dirSampleDist));

            /* ② 막힘 확인 */
            Vector3 prevPos = transform.position;
            float stuckT = 0f;
            bool moved = false;

            while (stuckT < stuckCheckTime && state == State.Flee)
            {
                stuckT += Time.deltaTime;
                t += Time.deltaTime;

                if ((transform.position - prevPos).sqrMagnitude >
                    stuckMoveEps * stuckMoveEps)
                { moved = true; break; }
                yield return null;
            }

            /* ③ 막혔으면 X 축 반대방향으로 한번 더 뛰기 */
            if (!moved && state == State.Flee)
            {
                float dirX = bestDir.x != 0f
                           ? -Mathf.Sign(bestDir.x)
                           : -Mathf.Sign(playertrans.position.x - transform.position.x);

                agent.SetDestination(transform.position + new Vector3(dirX, 0f) * dirSampleDist);
                yield return new WaitForSeconds(0.4f);
                t += 2f;
                continue;   // 다음 루프에서 재샘플
            }

            /* ④ 정상 이동 중이면 resampleCycle 후 경로 재계산 */
            yield return new WaitForSeconds(resampleCycle);
            t += resampleCycle;
        }

        ChangeState(State.Escaped);
    }

    protected override IEnumerator Escaped()
    {
        Play(DieAnim);              // "BeetleBurrow"
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}