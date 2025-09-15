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

    public CombatSuperState(MonsterContext c, MonsterStateMachine r) { ctx = c; root = r; }

    public void Enter()
    {
        if (!ctx.isCombat) ctx.isCombat = true;
        UpdatePresentationByRange();
        SelectAndRun();
    }

    public void Tick()
    {
        var r = ctx.hub.DecideDuringCombat(Time.deltaTime);
        if (r != Route.None)
        {
            if (r == Route.Return) ctx.IsFastReturn = ctx.hub.IsFastReturnRequested;
            Switch(r);
        }
        else
        {
            UpdatePresentationByRange();
        }
    }

    public void Exit() { Interrupt(); }

    // === 실행/선택 ===
    void SelectAndRun()
    {
        Interrupt();

        if (TryPickForced(out var forcedBeh, out bool forcedIsAttack))
        {
            SetPresentation(forcedIsAttack);
            Run(forcedBeh);
            return;
        }

        bool inMelee = IsInMelee();
        if (inMelee)
            ctx.animationHub?.SetTag(MonsterStateTag.CombatAttack, ctx);
        else
            ctx.animationHub?.SetTag(MonsterStateTag.CombatMove, ctx);
        SetPresentation(inMelee);

        IMonsterBehaviour beh = inMelee
            ? PickFirst(ctx.data.combatAttackBehaviours)
            : PickWeighted(ctx.data.combatMoveBehaviours, ctx.data.moveWeights);

        if (beh == null)
        {
            var alt = inMelee
                ? PickWeighted(ctx.data.combatMoveBehaviours, ctx.data.moveWeights)
                : PickFirst(ctx.data.combatAttackBehaviours);

            if (alt == null) { Switch(Route.Return); return; }
            beh = alt;
        }

        Run(beh);
    }

    void Run(IMonsterBehaviour beh)
    {
        currentBeh = beh;
        running = ctx.mono.StartCoroutine(RunWithHubPreempt(beh));
    }

    IEnumerator RunWithHubPreempt(IMonsterBehaviour beh)
    {
        var inner = beh.Execute(ctx);

        while (true)
        {
            var rr = ctx.hub.DecideDuringCombat(0f);
            if (rr != Route.None)
            {
                if (rr == Route.Return) ctx.IsFastReturn = ctx.hub.IsFastReturnRequested;
                Switch(rr);
                yield break;
            }

            if (!inner.MoveNext()) break;
            yield return inner.Current;
        }

        var post = ctx.hub.DecideDuringCombat(0f);
        if (post != Route.None)
        {
            if (post == Route.Return) ctx.IsFastReturn = ctx.hub.IsFastReturnRequested;
            Switch(post);
            yield break;
        }

        if (ctx.isCombat) SelectAndRun();
    }

    void Interrupt()
    {
        if (running != null)
        {
            ctx.mono.StopCoroutine(running);
            currentBeh?.OnInterrupt(ctx);
            running = null;
            currentBeh = null;
        }
    }

    void Switch(Route r)
    {
        Interrupt();
        ctx.isCombat = false;

        switch (r)
        {
            case Route.Return: root.ChangeState(new MonsterReturnState(ctx, root)); break;
            case Route.Detect: root.ChangeState(new MonsterDetectState(ctx, root)); break;
            default: root.ChangeState(new MonsterIdleState(ctx, root)); break;
        }
    }

    // === 선택 유틸 ===
    bool TryPickForced(out IMonsterBehaviour beh, out bool isAttackPick)
    {
        beh = null;
        isAttackPick = false;

        if (ctx.nextBehaviourIndex < 0) return false;

        int idx = ctx.nextBehaviourIndex;
        ctx.nextBehaviourIndex = -1;  // 소진

        // 공격 배열 영역
        var attacks = ctx.data.combatAttackBehaviours;
        if (attacks != null && idx >= 0 && idx < attacks.Length)
        {
            var b = attacks[idx];
            if (b && b.CanRun(ctx))
            {
                beh = b;
                isAttackPick = true;
                return true;
            }
        }

        // 이동 배열 영역
        var moves = ctx.data.combatMoveBehaviours;
        int mIdx = idx - (attacks?.Length ?? 0);
        if (moves != null && mIdx >= 0 && mIdx < moves.Length)
        {
            var b = moves[mIdx];
            if (b && b.CanRun(ctx))
            {
                beh = b;
                isAttackPick = false;
                return true;
            }
        }

        return false;
    }
    IMonsterBehaviour PickFirst(AttackBehaviourSO[] list)
    {
        if (list == null) return null;
        foreach (var b in list)
        {
            if (b && b.CanRun(ctx)) return b;
        }
        return null;
    }

    IMonsterBehaviour PickWeighted(AttackBehaviourSO[] list, float[] weights)
    {
        if (list == null || list.Length == 0 || weights == null || weights.Length < list.Length)
            return PickFirst(list);

        float total = 0f;
        var tmp = new float[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            bool ok = list[i] && list[i].CanRun(ctx);
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

    // === 표시/애니 ===
    bool IsInMelee()
    {
        if (!ctx.player) return false;
        return Vector2.Distance(ctx.transform.position, ctx.player.position) <= ctx.data.attackEnterDistance;
    }

    void UpdatePresentationByRange() => SetPresentation(IsInMelee());

    void SetPresentation(bool attackTag)
    {
        var tag = attackTag ? MonsterStateTag.CombatAttack : MonsterStateTag.CombatMove;
        ctx.indicator?.Show(tag);
    }
}