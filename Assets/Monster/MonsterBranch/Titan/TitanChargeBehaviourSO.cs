using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviours/Titan/Dash (Pure Transform 5f + StopOnHit + FarmHit)")]
public class TitanDashSO : AttackBehaviourSO
{
    [Header("Dash params")]
    public float windup = 1f;            // 시전 딜레이
    public float recover = 3f;           // ★ 후딜(요청)
    public float dashSpeed = 12f;          // m/s
    public float dashDistance = 5f;        // ★ 정확히 이 거리만큼 이동
    public float maxDashSeconds = 1.2f;    // 안전 상한

    [Header("Collision/Hit")]
    public float bodyRadius = 0.3f;        // 본체 반경(충돌 캐스트)
    public float skin = 0.02f;             // 벽 앞 안전 오프셋
    public int damage = 30;
    public float hitRadius = 1.0f;

    [Tooltip("장애물(벽) 판정 마스크 (미지정 시 ctx.obstacleMask 사용)")]
    public LayerMask obstacleMaskOverride;

    [Header("Hit mask (Player/Farm 등 타격 대상)")]
    public LayerMask hitMask;              // ex) Player, Farm 등
    public bool hitTriggersOnly = false;

    [Header("After-branch %")]
    [Range(0, 100)] public int repeatDashPercent = 80; // 80% 재돌진

    [Header("Reattach to NavMesh (after dash ends)")]
    public float reattachRadius = 2f;      // 가까운 NavMesh 위치 스냅 반경

    // 재사용 버퍼(할당 줄이기)
    readonly Collider2D[] _hits = new Collider2D[16];

    public override bool CanRun(MonsterContext ctx) => ctx.player != null;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        if (!ctx.player) yield break;

        // 1) 방향 계산 & 즉시 바라보기
        Vector3 start = ctx.transform.position;
        Vector3 toP = (ctx.player.position - start);
        if (toP.sqrMagnitude < 1e-6f) yield break;
        Vector2 dir = ((Vector2)toP).normalized;
        FaceImmediate(ctx, dir);

        // 2) NavMeshAgent 간섭 제거
        bool hadAgent = ctx.agent != null;
        bool prevEnabled = hadAgent && ctx.agent.enabled;
        if (hadAgent)
        {
            ctx.SafeStopAgent();
            ctx.agent.enabled = false; // ← 돌진 동안 완전 비활성
        }

        // 3) 준비 모션
        ctx.anim?.Play("DashPrep");
        yield return new WaitForSeconds(windup);

        // 4) 직선 돌진 (Transform 이동 + CircleCast로 충돌 체크)
        ctx.anim?.Play("Dash");
        float moved = 0f, elapsed = 0f;
        bool hitPlayerOnce = false;
        var hitFarmSet = new HashSet<object>(); // 같은 프레임 중복타격 방지

        LayerMask obstacleMask = (obstacleMaskOverride.value != 0) ? obstacleMaskOverride : ctx.obstacleMask;

        while (moved < dashDistance && elapsed < maxDashSeconds)
        {
            elapsed += Time.deltaTime;

            float step = Mathf.Min(dashSpeed * Time.deltaTime, dashDistance - moved);
            Vector3 cur = ctx.transform.position;
            Vector3 stepVec = (Vector3)(dir * step);

            // 4-1) 충돌 검사: 현재 위치에서 dir로 step만큼 원캐스트 → 맞으면 즉시 정지
            RaycastHit2D stopHit = Physics2D.CircleCast(cur, bodyRadius, dir, step, obstacleMask);
            if (stopHit)
            {
                Vector3 stopPos = (Vector3)stopHit.point - (Vector3)(dir * (bodyRadius + skin));
                ctx.transform.position = stopPos;
                FaceImmediate(ctx, dir);
                break; // ★ 충돌 → 그 자리에서 멈춤
            }

            // 4-2) 자유 이동
            ctx.transform.position = cur + stepVec;
            moved += step;

            // 4-3) 히트 판정(플레이어 1회, 자원 다회 가능)
            // 플레이어
            if (!hitPlayerOnce && ctx.player &&
                Vector2.Distance(ctx.transform.position, ctx.player.position) <= hitRadius)
            {
                var def = PlayerData.Instance ? PlayerData.Instance.GetComponent<PlayerDefense>() : null;
                int final = damage;
                if (def) final = def.ResolveGuard(damage);
                if (PlayerData.Instance) PlayerData.Instance.HPValueChange(-final);
                hitPlayerOnce = true;
            }

            // 자원(Farm) — DoSlashBox와 같은 컨셉
            int n = Physics2D.OverlapCircleNonAlloc(ctx.transform.position, hitRadius, _hits, hitMask);
            for (int i = 0; i < n; i++)
            {
                var col = _hits[i];
                if (!col || !col.enabled) continue;

                if (hitTriggersOnly && !col.isTrigger) continue;
                if (col.transform == ctx.transform) continue; // 자기 자신 제외

                if (col.CompareTag("Farm") && col.TryGetComponent(out ResourceNodeBase f))
                {
                    if (hitFarmSet.Add(f))
                        f.Damage(damage); // ★ 자원 데미지 → 드랍 유도
                    continue;
                }

                // 필요 시 다른 대상들 추가 가능 (예: Breakable 등)
            }

            // 4-4) 시선 유지(즉시)
            FaceImmediate(ctx, dir);
            yield return null;
        }

        // 5) 재부착: 다음 상태에서 NavMeshAgent를 쓸 수 있으니 한 번만 복귀
        if (hadAgent)
        {
            ctx.agent.enabled = prevEnabled || true; // 켜두고
            if (NavMesh.SamplePosition(ctx.transform.position, out var snap, reattachRadius, NavMesh.AllAreas))
                ctx.agent.Warp(snap.position);
            else
                ctx.EnsureAgentReady(reattachRadius);
            ctx.agent.isStopped = true; // 다음 상태가 직접 재개
        }

        // 6) ★ 후딜
        ctx.anim?.Play("DashRecover");
        if (recover > 0f)
            yield return new WaitForSeconds(recover);

        // 7) 이후 분기 (네 기존 로직과 호환)
        ctx.patternCount++;
        if (ctx.patternCount >= ctx.PatternEveryRest)
        {
            ctx.patternCount = 0;
            ctx.nextBehaviourIndex = 2; // Rest
            yield break;
        }

        int atkCount = ctx.data.combatAttackBehaviours?.Length ?? 0;
        int approachIdx = atkCount + 0; // 이동배열 0번(Approach)
        int roll = Random.Range(0, 100);
        ctx.nextBehaviourIndex = (roll < repeatDashPercent) ? 1 : approachIdx;
    }

    // 애니 파라미터/Flip을 즉시 반영 (SetForward는 기다림이 있어 대시에 부적합)
    static void FaceImmediate(MonsterContext ctx, Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) return;
        if (ctx.anim)
        {
            ctx.anim.SetFloat("DirX", dir.x);
            ctx.anim.SetFloat("DirY", dir.y);
        }
        if (ctx.sr) ctx.sr.flipX = (dir.x < 0f);
    }
}