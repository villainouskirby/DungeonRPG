using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Hound/Charge Attack")]
public class HoundChargeAttackBehaviourSO : AttackBehaviourSO
{
    public float chargeDistance = 4f;
    public float windupTime = 0.3f;
    public float recoverTime = 0.4f;
    public float dashSpeed = 10f;
    public int damage = 20;

    [Header("Attack Range")]
    public string playerTag = "Player";
    public float sweepRadius = 0.5f;

    public override bool CanRun(MonsterContext ctx)
        => Vector2.Distance(ctx.transform.position, ctx.player.position) <= chargeDistance;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        if (!ctx.player) yield break;
        SoundManager.Instance.StopLoopSound("SFX_HoundRun");
        Vector2 dir = (ctx.player.position - ctx.transform.position).normalized;
        /* 1) 준비 */
        ctx.SetForward(dir);
        ctx.agent.isStopped = true;
        ctx.agent.velocity = Vector3.zero;
        ctx.animationHub?.SetTag(MonsterStateTag.Idle, ctx);
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(windupTime / 3);
            ctx.SetForward(dir);
        }


        /* 2) 돌진 */
        ctx.SetForward(dir);

        ctx.animationHub?.SetTag(MonsterStateTag.CombatAttack, ctx);
        SoundManager.Instance.PlaySound3D(
                    "SFX_HoundChargeAttack",
                    ctx.transform,
                    0f,
                    false,
                    SoundType.SFX,
                    true,
                    1.5f,
                    25f
                );
        float travelled = 0f;
        bool hitApplied = false;
        bool stopOnHit = false;

        while (travelled < chargeDistance && !stopOnHit)
        {
            float step = dashSpeed * Time.deltaTime;
            Vector2 prevPos = ctx.transform.position;
            Vector2 nextPos = prevPos + dir * step;

            // 직선 스윕으로 충돌 체크
            Vector2 delta = nextPos - prevPos;
            float dist = delta.magnitude;

            if (dist > 1e-4f)
            {
                var hits = Physics2D.CircleCastAll(prevPos, sweepRadius, delta.normalized, dist);
                for (int i = 0; i < hits.Length; i++)
                {
                    var h = hits[i];
                    if (h.collider && h.collider.CompareTag(playerTag))
                    {
                        // 플레이어 무적 여부 확인
                        var pc = h.collider.GetComponent<PlayerController>();
                        bool invincible = pc && pc.isInvincible;

                        if (!invincible && !hitApplied)
                        {
                            ApplyDamageToPlayer(damage); // 가드 반영 포함
                            hitApplied = true;

                            // 그 자리에서 멈춤
                            ctx.transform.position = nextPos; // 충돌 지점에 위치 고정
                            stopOnHit = true;                 // 루프 탈출
                        }

                        // 무적이면 그냥 통과(아무 것도 안 함)
                        break;
                    }
                }
            }

            if (!stopOnHit)
            {
                // 이동 지속
                ctx.transform.position = nextPos;
                travelled += step;
                yield return null;
            }
        }

        /* 4) 후딜레이 */
        yield return new WaitForSeconds(0.3f);
        ctx.animationHub?.SetTag(MonsterStateTag.Idle, ctx);
        yield return new WaitForSeconds(recoverTime-0.3f);
        ctx.agent.isStopped = false;

        // 돌진 끝난 후 플레이어 쪽으로 다시 방향 갱신
        if (ctx.player)
        {
            Vector2 toPlayer = (ctx.player.position - ctx.transform.position).normalized;
            ctx.SetForward(toPlayer);
        }

        int roll = Random.Range(0, 100);
        if (roll < 30) ctx.nextBehaviourIndex = 0;   // 근접공격
        else ctx.nextBehaviourIndex = 3;             // 백점프
    }

    public override void OnInterrupt(MonsterContext ctx)
    {
        ctx.agent.isStopped = false;   // 중단 시 이동 해제
    }
    static void ApplyDamageToPlayer(int rawDamage)
    {
        if (!PlayerData.Instance) return;

        int finalDamage = rawDamage;

        var defense = PlayerData.Instance.GetComponent<PlayerDefense>();
        if (defense) finalDamage = defense.ResolveGuard(rawDamage);

        PlayerData.Instance.HPValueChange(-finalDamage);
    }
}