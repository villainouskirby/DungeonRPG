using System.Collections;
using System.Collections.Generic;
using UnityEditor.Connect;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
public sealed class CombatSuperState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;

    Coroutine running;
    IMonsterBehaviour currentBeh;
    float lostHold;   // 전투 해제 누적

    public CombatSuperState(MonsterContext c, MonsterStateMachine r) { ctx = c; root = r; }

    public void Enter()
    {
        if (!ctx.isCombat) ctx.isCombat = true;
        lostHold = 0f;
        SelectAndRun();
    }

    public void Tick()
    {
        // 전투 유지/해제 판정
        if (GuardDisengage(Time.deltaTime)) return;
    }

    public void Exit() { Interrupt(); }

    // 전투 해제 가드 
    bool GuardDisengage(float dt)
    {
        // 스포너 과거리 → Return
        float dSpawn = Vector2.Distance(ctx.transform.position, ctx.spawner);
        if (dSpawn > ctx.data.maxSpawnerDist)
        {
            ctx.IsFastReturn = true;
            ctx.isCombat = false;
            Switch(Route.Return);
            return true;
        }

        bool see = ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle);
        bool hear = ctx.CanHearPlayer(ctx.data.hearRange);
        float dPlayer = ctx.player ? Vector2.Distance(ctx.transform.position, ctx.player.position) : Mathf.Infinity;

        if (see || hear) lostHold = 0f;
        else lostHold += dt;

        if (dPlayer > ctx.data.lostDistance && lostHold >= ctx.data.disengageHoldSeconds)
        {
            ctx.isCombat = false;  // 전투 종료
            Switch(Route.Detect);  // 흔적 수사로 복귀
            return true;
        }
        return false;
    }

    // 선택/실행
    void SelectAndRun()
    {
        Interrupt();

        bool inMelee = ctx.player &&
            Vector2.Distance(ctx.transform.position, ctx.player.position) <= ctx.data.attackEnterDistance;

        var bucket = inMelee ? ctx.data.combatAttackBehaviours
                             : ctx.data.combatMoveBehaviours;

        IMonsterBehaviour beh = inMelee
            ? PickFirstReady(bucket)                  // 공격은 순서 우선(혹은 별 규칙)
            : PickWeightedReady(bucket, ctx.data.moveWeights);

        if (beh == null)
        {
            // 대체: 이동 버킷/공격 버킷 서로 교차 시도
            var alt = inMelee ? PickWeightedReady(ctx.data.combatMoveBehaviours, ctx.data.moveWeights)
                              : PickFirstReady(ctx.data.combatAttackBehaviours);
            if (alt == null) { Switch(Route.Return); return; }
            beh = alt;
        }

        currentBeh = beh;
        running = ctx.mono.StartCoroutine(RunWithPreempt(beh));
    }

    IEnumerator RunWithPreempt(IMonsterBehaviour beh)
    {
        var inner = beh.Execute(ctx);

        while (true)
        {
            if (GuardDisengage(0f)) yield break;   // 프레임 단위 프리엠프

            if (!inner.MoveNext()) break;
            yield return inner.Current;
        }

        // 종료 직후에도 가드 체크
        if (GuardDisengage(0f)) yield break;

        // 쿨다운 부여
        float cd = (beh is IWithCooldown w) ? w.CooldownSeconds : 0.15f;
        ctx.SetCooldown(beh, cd);

        // 다음 선택
        if (ctx.isCombat) SelectAndRun();
    }

    void Interrupt()
    {
        if (running != null)
        {
            ctx.mono.StopCoroutine(running);

            // 인터럽트에도 쿨 부여(정의 쿨의 절반, 최소 0.2s)
            if (currentBeh is IWithCooldown w)
                ctx.SetCooldown(currentBeh, Mathf.Max(0.2f, w.CooldownSeconds * 0.5f));

            currentBeh?.OnInterrupt(ctx);
            running = null;
            currentBeh = null;
        }
    }

    void Switch(Route r)
    {
        Interrupt();
        switch (r)
        {
            case Route.Return: root.ChangeState(new MonsterReturnState(ctx, root)); break;
            case Route.Detect: root.ChangeState(new MonsterDetectState(ctx, root)); break;
            default: root.ChangeState(new MonsterIdleState(ctx, root)); break;
        }
    }

    // 픽커들
    IMonsterBehaviour PickFirstReady(AttackBehaviourSO[] list)
    {
        if (list == null) return null;
        foreach (var b in list)
        {
            if (!b) continue;
            if (!ctx.IsReady(b)) continue;
            if (!b.CanRun(ctx)) continue;
            return b;
        }
        return null;
    }

    IMonsterBehaviour PickWeightedReady(AttackBehaviourSO[] list, float[] weights)
    {
        if (list == null || weights == null || list.Length == 0 || weights.Length < list.Length) return PickFirstReady(list);

        // 가중치에서 쿨/조건 불가 항목은 0으로
        float total = 0f;
        var tmp = new float[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            bool ok = list[i] && ctx.IsReady(list[i]) && list[i].CanRun(ctx);
            tmp[i] = ok ? Mathf.Max(0f, weights[i]) : 0f;
            total += tmp[i];
        }
        if (total <= 0f) return null;

        float pick = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < list.Length; i++)
        {
            acc += tmp[i];
            if (pick <= acc) return list[i];
        }
        return null;
    }
}