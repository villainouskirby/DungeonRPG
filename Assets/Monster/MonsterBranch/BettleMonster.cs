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

    public override void TakeDamage(float dmg)
    {
        ChangeState(State.Flee);
        hp -= dmg;
        HealthBarManager.Instance?.UpdateBar(this, GetHPRatio());
        Debug.Log("현재 몬스터 체력" + hp);
        if (hp <= 0 && state != State.Killed) ChangeState(State.Killed);
    }
    // Flee 코루틴 (4초 도망 → 1초 burrow → 소멸) 
    protected override IEnumerator Flee()
    {
        const float fleeTime = 4f;     // Flee 전체 한도
        const float checkStuck = 0.2f;   // “막힘” 판단 시간
        const float stuckMoveEps = 0.05f;  // 움직임 판정 오차
        const float fallbackRun = 1f;     // fallback 달리기 시간
        const float sampleRadius = 4f;     // 목적지까지 거리

        float t = 0f;
        Play(RunAnim);
        agent.speed = data.FleeSpeed;

        while (t < fleeTime && state == State.Flee)
        {
            // flee 목적지 계산 
            Vector2 away = (transform.position - player.position).normalized;
            Vector2 bestDir = away;                // fallback
            float bestScore = -1f;

            const int numSamples = 16;
            for (int i = 0; i < numSamples; ++i)
            {
                float rad = Mathf.Deg2Rad * (360f / numSamples) * i;
                Vector2 dir = new(Mathf.Cos(rad), Mathf.Sin(rad));

                if (Vector2.Dot(dir, away) <= 0f) continue;     // 플레이어와 반대가 아니면 무시

                float score = Mathf.Abs(dir.x);                 // “x 성분”이 클수록 우선
                if (score > bestScore) { bestScore = score; bestDir = dir; }
            }

            // 이동 시작 
            Vector3 dest = transform.position + (Vector3)(bestDir.normalized * sampleRadius);
            agent.SetDestination(dest);

            // 0.2 초 동안 움직였는지 감시
            Vector3 prev = transform.position;
            float stuckT = 0f;
            bool moved = false;

            while (stuckT < checkStuck && state == State.Flee)
            {
                stuckT += Time.deltaTime;
                t += Time.deltaTime;

                if ((transform.position - prev).sqrMagnitude > stuckMoveEps * stuckMoveEps)
                { moved = true; break; }          // 정상 이동 시작 → 루프 탈출
                yield return null;
            }

            //“막다름” 판정 시 X‑축 반대로 1 초간 달림
            if (!moved && state == State.Flee)
            {
                // 현재 bestDir.x 가 0 이면 플레이어 기준으로 방향 결정
                float dirX = bestDir.x != 0f
                             ? -Mathf.Sign(bestDir.x)
                             : -Mathf.Sign(player.position.x - transform.position.x);

                Vector3 fallbackDir = new(dirX, 0f, 0f);
                Vector3 fallbackDest = transform.position + fallbackDir * sampleRadius;
                agent.SetDestination(fallbackDest);

                float fbT = 0f;
                while (fbT < fallbackRun && state == State.Flee)
                {
                    fbT += Time.deltaTime;
                    t += Time.deltaTime;
                    yield return null;
                }

                // 다음 루프에서 새 경로 샘플링
                continue;
            }

            // 정상 이동 중이면 1 초마다 목적지 재계산
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

    protected override IEnumerator Escaped()
    {
        Play(DieAnim);              // "BeetleBurrow"
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}