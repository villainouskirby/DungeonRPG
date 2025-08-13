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

    Coroutine running;
    IMonsterBehaviour currentBeh;   // ← 인터럽트용

    int idx;                          // 순환 인덱스
    float lostTimer;

    public CombatSuperState(MonsterContext c, MonsterStateMachine r)
    { ctx = c; root = r; }

    public void Enter()
    {
        Debug.Log($"{ctx.data.monsterName} ▶ Combat 진입");
        lostTimer = 0f;
        PickAndRun();
    }
    public void Exit() => Interrupt();
    public void Tick()
    {
        float dist = Vector2.Distance(ctx.transform.position, ctx.player.position);
        bool see = ctx.CanSeePlayer(ctx.data.lostDistance, ctx.data.sightAngle);

        if (!see || dist > ctx.data.lostDistance)
            lostTimer += Time.deltaTime;
        else
            lostTimer = 0f;

        if (lostTimer >= 1f)            // 1초 이상 놓치면 Return
        {
            Debug.Log($"{ctx.data.monsterName} ▶ Combat 종료 dist:{dist:F1} see:{see}");
            Interrupt();
            root.ChangeState(new MonsterReturnState(ctx, root));
        }
        if(Vector2.Distance(ctx.transform.position, ctx.spawner) > ctx.data.maxSpawnerDist)
        {
            Debug.Log($"{ctx.data.monsterName} ▶ Combat 종료 스포너와 멀어졌음");
            Interrupt();
            ctx.IsFastReturn = true;
            root.ChangeState(new MonsterReturnState(ctx, root));
        }
    }

    // 내부

    void PickAndRun()
    {
        Interrupt();

        var list = ctx.data.attackBehaviours;
        if (list == null || list.Length == 0)
        { ctx.sm.ChangeState(new MonsterReturnState(ctx, ctx.sm)); return; }

        /* attackBehaviours 배열을 한 바퀴 돌며 CanRun==true 찾기 */
        for (int i = 0; i < list.Length; ++i)
        {
            idx = (idx + 1) % list.Length;
            var beh = list[idx];
            if (beh != null && beh.CanRun(ctx))
            {
                Run(beh);
                return;
            }
        }

        /* 실행할 게 없다면 귀환 */
        root.ChangeState(new MonsterReturnState(ctx, root));
    }

    void Run(IMonsterBehaviour beh)
    {
        currentBeh = beh;
        running = ctx.mono.StartCoroutine(Wrap());

        IEnumerator Wrap()
        {
            yield return beh.Execute(ctx);
            PickAndRun();            // 종료 후 다음 패턴 선정
        }
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
}