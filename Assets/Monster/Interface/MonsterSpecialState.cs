using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterSpecialState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;

    int curIdx = -1;              // specialBehaviours 순환 인덱스
    Coroutine running;
    IMonsterBehaviour curBeh;

    public MonsterSpecialState(MonsterContext c, MonsterStateMachine r) { ctx = c; root = r; }

    public void Enter() => PickAndRun();
    public void Exit() => Interrupt();
    public void Tick()
    {
    }
    void PickAndRun()
    {
        Interrupt();

        var list = ctx.data.specialBehaviours;
        if (list == null || list.Length == 0)
        { root.ChangeState(new MonsterIdleState(ctx, root)); return; }

        for (int i = 0; i < list.Length; ++i)
        {
            curIdx = (curIdx + 1) % list.Length;
            var beh = list[curIdx];
            if (beh != null && beh.CanRun(ctx))
            {
                curBeh = beh;
                running = ctx.mono.StartCoroutine(Wrap());
                return;
            }
        }

        // 실행할 게 없다면 Idle
        root.ChangeState(new MonsterIdleState(ctx, root));
    }

    IEnumerator Wrap()
    {
        yield return curBeh.Execute(ctx);
        PickAndRun();                    // 다음 특수 행동
    }

    void Interrupt()
    {
        if (running != null)
        {
            ctx.mono.StopCoroutine(running);
            curBeh?.OnInterrupt(ctx);
            running = null;
            curBeh = null;
        }
    }
}