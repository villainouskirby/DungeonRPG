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
    int hitMark; // 진입 시점 피격 시리얼 저장
    public MonsterSpecialState(MonsterContext c, MonsterStateMachine r) { ctx = c; root = r; }

    public void Enter()
    {
        hitMark = ctx.hitSerial;  // 현재 값 저장
        PickAndRun();
    }
    public void Exit() => Interrupt();
    public void Tick()
    {
        // 피격 발생 감지: 값이 바뀌면 스턴으로
        if (ctx.hitSerial != hitMark)
        {
            Interrupt(); // 실행 중 특수 행동 코루틴 안전 종료
            // 스턴 길이: 데이터가 있으면 그 값, 없으면 0.3f
            float stunSec = (ctx.data && ctx is { }) ? Mathf.Max(0.05f, ctx.data.defaultHitStunSeconds) : 0.3f;

            // 스턴을 '오버레이'로 푸시하고, 끝나면 Flee로 강제 전환
            root.PushState(new MonsterStunState(ctx, root, stunSec, goToFleeOnEnd: true));
            hitMark = ctx.hitSerial; // (중복 진입 방지)
            return;
        }
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