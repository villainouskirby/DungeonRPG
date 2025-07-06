using System.Collections;
using System.Collections.Generic;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
public sealed class CombatSuperState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;

    int current;                 // attackBehaviours 인덱스
    Coroutine running;           // 현재 돌고 있는 코루틴

    public CombatSuperState(MonsterContext c, MonsterStateMachine r)
    { ctx = c; root = r; }

    /* ───────── 생명주기 ───────── */

    public void Enter()
    {
        current = 0;
        NextPattern();
    }

    public void Tick()
    {
        // 전역 이탈 조건 - 멀어지면 종료
        if (!ctx.CanSeePlayer(ctx.data.lostDistance))
        {
            StopCurrent();
            root.ChangeState(new MonsterReturnState(ctx, root));
            return;
        }
    }

    public void Exit() => StopCurrent();

    // 내부 메서드 

    void NextPattern()
    {
        if (ctx.data.attackBehaviours == null || ctx.data.attackBehaviours.Length == 0)
        { root.ChangeState(new MonsterReturnState(ctx, root)); return; }

        var beh = ctx.data.attackBehaviours[current];
        current = (current + 1) % ctx.data.attackBehaviours.Length;

        running = ctx.mono.StartCoroutine(RunPattern(beh));
    }

    IEnumerator RunPattern(IMonsterBehaviour beh)
    {
        yield return beh.Execute(ctx);

        // 패턴 종료 후 바로 다음
        NextPattern();
    }

    void StopCurrent()
    {
        if (running != null)
        {
            ctx.mono.StopCoroutine(running);
            running = null;
        }
    }
}