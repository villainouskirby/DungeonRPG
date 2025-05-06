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
        if (col.CompareTag("Player") && CanSeePlayer(col.transform, data.sightDistance))
        {
            player = col.transform;
            if (state is not (State.Killed or State.Escaped))
                ChangeState(State.Flee);    // ▶ Combat 건너뛰고 바로 Flee
        }
    }
    protected virtual void OnTriggerEnter2D(Collider2D col) { return; }
    protected override IEnumerator Combat()
    {
        ChangeState(State.Flee);
        yield break;
    }
    // 2. Flee 코루틴 구현 (4초 도망 → 1초 burrow → 소멸) 
    protected override IEnumerator Flee()
    {
        float fleeTime = 4f;
        float t = 0f;

        Play(RunAnim);
        agent.speed = data.FleeSpeed;

        while (t < fleeTime && state == State.Flee)
        {
            // ── 가장 'x' 성분이 큰, 플레이어와 반대 방향 샘플 찾기 ──
            Vector2 away = (transform.position - player.position).normalized;
            Vector2 bestDir = away;             // fallback
            float bestScore = -1f;

            const int numSamples = 16;
            for (int i = 0; i < numSamples; i++)
            {
                float rad = Mathf.Deg2Rad * (360f / numSamples) * i;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                // 플레이어와 멀어지는 방향만 고려
                if (Vector2.Dot(dir, away) <= 0f) continue;

                float score = Mathf.Abs(dir.x);   // x 우선
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = dir;
                }
            }

            Vector3 dest = transform.position + (Vector3)(bestDir.normalized * 4f);
            agent.SetDestination(dest);

            // 1초마다 목적지 재계산
            float intTimer = 0f;
            while (intTimer < 1f && state == State.Flee)
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