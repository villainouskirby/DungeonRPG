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
    CancellationTokenSource cts;

    Coroutine running;
    IMonsterBehaviour currentBeh;   // ← 인터럽트용

    int idx;                          // 순환 인덱스
    float lostTimer;

    public CombatSuperState(MonsterContext c, MonsterStateMachine r)
    { ctx = c; root = r; }

    public void Enter()
    {
        Debug.Log($"{ctx.data.monsterName} ▶ Combat 진입");
        if (ctx.alert && ctx.data.exclamationSprite)
        {
            ctx.alert.sprite = ctx.data.exclamationSprite;
            ctx.alert.color = Color.red;
            ctx.alert.gameObject.SetActive(true);
        }

        lostTimer = 0f;
        PickAndRun();

        cts = new CancellationTokenSource();
        StartDisengageWatcherAsync(cts.Token).Forget();
    }
    public void Exit()
    {
        Interrupt();

        // 감시 태스크 취소
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        // 아이콘은 Return/Detect에서 상태별로 관리한다면 꺼도 되고 유지해도 됨
        if (ctx.alert) ctx.alert.gameObject.SetActive(false);
    }
    public void Tick()
    {
        if (Vector2.Distance(ctx.transform.position, ctx.spawner) > ctx.data.maxSpawnerDist)
        {
            Debug.Log($"{ctx.data.monsterName} ▶ Combat 종료 스포너와 멀어졌음");
            Interrupt();

            // 전투 해제 감시 태스크 취소 (Enter에서 만든 cts가 있다면)
            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            ctx.IsFastReturn = true;
            root.ChangeState(new MonsterReturnState(ctx, root));
            return;
        }
    }

    // 내부
    bool CombatPredicate()
        => ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle)
        || ctx.CanHearPlayer(ctx.data.hearRange);
    async UniTaskVoid StartDisengageWatcherAsync(CancellationToken token)
    {
        // 진행도 콜백(선택): 해제 게이지/알파 등 UI 연출 가능
        void Progress(float t)
        {
            // 예시) 알파 깜빡임/게이지 바 등
            // if (ctx.alert) ctx.alert.color = Color.Lerp(Color.red, Color.white, t);
        }

        bool lostLongEnough = await ConditionAwaiter.HoldFalseContinuously(
            ctx.data.disengageHoldSeconds,
            CombatPredicate,
            Progress,
            token);

        if (token.IsCancellationRequested || !lostLongEnough) return;

        Debug.Log($"{ctx.data.monsterName} ▶ Combat 종료: 전투 조건 미유지 {ctx.data.disengageHoldSeconds:F1}s");
        Interrupt();
        root.ChangeState(new MonsterReturnState(ctx, root));
    }

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