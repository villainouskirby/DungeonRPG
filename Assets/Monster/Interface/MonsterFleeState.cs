using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MonsterFleeState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;
    Coroutine running;

    public MonsterFleeState(MonsterContext c, MonsterStateMachine r) { ctx = c; root = r; }

    public void Enter()
    {
        /* HP 낮아졌을 때만 fleeBehaviours 실행하도록 할 수도 있음 */
        if (ctx.data.fleeBehaviours?.Length > 0)
        {
            var beh = ctx.data.fleeBehaviours[Random.Range(0, ctx.data.fleeBehaviours.Length)];
            running = ctx.mono.StartCoroutine(RunFlee(beh));
        }
        else   // 정의된 Flee 패턴이 없다면 단순 귀환
        {
            root.ChangeState(new MonsterReturnState(ctx, root));
        }
    }

    IEnumerator RunFlee(IMonsterBehaviour beh)
    {
        yield return beh.Execute(ctx);           // 도망 패턴 실행

        /* 안전거리 도달 여부 판단 후 Return or Idle */
        bool nearSpawner = ctx.spawner &&
             Vector2.Distance(ctx.transform.position, ctx.spawner.position) <= ctx.data.nearSpawnerDist;

        root.ChangeState(nearSpawner
                        ? new MonsterIdleState(ctx, root)
                        : new MonsterReturnState(ctx, root));
    }

    public void Tick() { }    // fleeBehaviours 안에서 직접 이동
    public void Exit()
    {
        if (running != null) ctx.mono.StopCoroutine(running);
    }
}