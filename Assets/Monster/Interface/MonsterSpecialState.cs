using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterSpecialState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;
    Coroutine running;

    public MonsterSpecialState(MonsterContext c, MonsterStateMachine r) { ctx = c; root = r; }

    public void Enter()
    {
        if (ctx.data.specialBehaviour == null)
        {
            root.ChangeState(new MonsterIdleState(ctx, root));
            return;
        }
        running = ctx.mono.StartCoroutine(RunSpecial());
    }

    IEnumerator RunSpecial()
    {
        yield return ctx.data.specialBehaviour.Execute(ctx);
        root.ChangeState(new MonsterIdleState(ctx, root));   // 특수행동 끝나면 Idle 복귀
    }

    public void Tick() { /* 일반적으로 아무 것도 하지 않음 */ }

    public void Exit()
    {
        if (running != null)
        {
            ctx.data.specialBehaviour.OnInterrupt(ctx);
            ctx.mono.StopCoroutine(running);
        }
    }
}