using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
public sealed class MonsterIdleState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float restTimer;
    float detectGate;   // Detect 전 대기 누적
    float returnGate;   // Return 전 대기 누적

    public MonsterIdleState(MonsterContext c, MonsterStateMachine m)
    { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Idle);
        ctx.houndanimPlayer?.SetTag(MonsterStateTag.Idle, ctx);
        ctx.cleaneranimPlayer?.SetTag(MonsterStateTag.Idle, ctx);
        if (!ctx.data.canMove)         // 고정형 Idle 유지
            return;
        ctx.anim.Play("Idle");
        restTimer = UnityEngine.Random.Range(0.5f, 2f);
        detectGate = 0f;
        returnGate = 0f;
    }

    public void Tick()
    {
        // TODO : 현재는 그냥 순서대로 가중치를 둬서 로직을 변경하지만 가중치를 설정해 분기 필요
        Route r = ctx.hub.Decide(Time.deltaTime);
        if (TryRoute(r)) return;

        restTimer -= Time.deltaTime;
        if (restTimer > 0f) return;

        // 배회 목적지 선정
        Vector2 rnd = UnityEngine.Random.insideUnitCircle * ctx.data.wanderRadius;
        Vector3 dest = ctx.transform.position + (Vector3)rnd;

        ctx.agent.speed = ctx.data.detectSpeed;
        ctx.agent.SetDestination(dest);
        ctx.anim.Play("Walk");

        machine.ChangeState(new MonsterWanderState(ctx, machine));
    }
    bool TryRoute(Route r)
    {
        switch (r)
        {
            case Route.Return:
                ctx.IsFastReturn = ctx.hub.IsFastReturnRequested;
                machine.ChangeState(new MonsterReturnState(ctx, machine)); return true;
            case Route.Detect:
                machine.ChangeState(new MonsterDetectState(ctx, machine)); return true;
            case Route.Special:
                machine.ChangeState(new MonsterSpecialState(ctx, machine)); return true;
            case Route.Attack:
                machine.ChangeState(new CombatSuperState(ctx, machine)); return true;
            case Route.Trace:
                machine.ChangeState(new MonsterTraceState(ctx, machine)); return true;
            case Route.Flee:
                machine.ChangeState(new MonsterFleeState(ctx, machine)); return true;
        }
        return false; // None
    }
    public void Exit() { }
}

// WanderState : 이동만 담당, 도착하면 Idle 로 복귀
public sealed class MonsterWanderState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float timeout = 1f;
    float detectGate;

    public MonsterWanderState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter() { if (!ctx.data.canMove) return; }         // 고정형 Idle 유지return;
    public void Tick()
    {
        Route r = ctx.hub.Decide(Time.deltaTime);
        if (r != Route.None)
        {
            // 귀환/수사/특수/전투/추적/도주 등 공통 처리
            SwitchByRoute(r);
            return;
        }
        bool arrived = !ctx.agent.pathPending &&
                      ctx.agent.remainingDistance <= ctx.agent.stoppingDistance;

        bool blocked = !ctx.agent.pathPending &&
                       ctx.agent.pathStatus != NavMeshPathStatus.PathComplete;

        timeout -= Time.deltaTime;
        if (arrived || blocked || timeout <= 0f)
        {
            machine.ChangeState(new MonsterIdleState(ctx, machine));
            return;
        }
    }
    void SwitchByRoute(Route r)
    {
        switch (r)
        {
            case Route.Return: ctx.IsFastReturn = ctx.hub.IsFastReturnRequested; machine.ChangeState(new MonsterReturnState(ctx, machine)); break;
            case Route.Detect: machine.ChangeState(new MonsterDetectState(ctx, machine)); break;
            case Route.Special: machine.ChangeState(new MonsterSpecialState(ctx, machine)); break;
            case Route.Attack: machine.ChangeState(new CombatSuperState(ctx, machine)); break;
            case Route.Trace: machine.ChangeState(new MonsterTraceState(ctx, machine)); break;
            case Route.Flee: machine.ChangeState(new MonsterFleeState(ctx, machine)); break;
        }
    }
    public void Exit() { }
}

// DetectState
public sealed class MonsterDetectState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;
    CancellationTokenSource cts;

    const float hearInterval = 0.5f;   // 청각 체크 주기
    const float chaseTimeout = 5f;     // 최근 소리 후 추적 유지 시간

    float hearTimer;    // 0.5초 타이머
    float chaseTimer;   // 5초 타이머
    Vector3 targetPos;  // 마지막 들린 위치
    float returnGate;

    public MonsterDetectState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Detect);
        ctx.houndanimPlayer?.SetTag(MonsterStateTag.Detect, ctx);
        ctx.cleaneranimPlayer?.SetTag(MonsterStateTag.Detect, ctx);
        ctx.anim.Play("Walk");
        ctx.agent.speed = ctx.data.detectSpeed;

        targetPos = (ctx.hub != null && ctx.hub.LastNoisePos != Vector3.zero)
                       ? ctx.hub.LastNoisePos
                       : (ctx.player ? ctx.player.position : ctx.transform.position);
        ctx.agent.SetDestination(targetPos);


        hearTimer = 0f;
        chaseTimer = chaseTimeout;
        returnGate = 0f;

        // 아이콘 초기화(물음표 + 흰색)
        if (ctx.alert && ctx.data.questionSprite)
        {
            ctx.alert.sprite = ctx.data.questionSprite;
            ctx.alert.color = ctx.data.questionStartColor; // white
            ctx.alert.gameObject.SetActive(true);
        }

        cts = new CancellationTokenSource();

        // “전투 조건 2초 연속 유지” 감시 시작
        StartAggroWatcherAsync(cts.Token).Forget();
    }
    public void Tick()
    {
        if (ctx.isCombat)
        {
            machine.ChangeState(new CombatSuperState(ctx, machine));
            return;
        }

        // 0) 스포너 거리 초과 → Return 게이트
        if (Vector2.Distance(ctx.transform.position, ctx.spawner) > ctx.data.maxSpawnerDist)
        {
            returnGate += Time.deltaTime;
            if (returnGate >= ctx.data.returnGateDelay)
            {
                ctx.IsFastReturn = true;
                machine.ChangeState(new MonsterReturnState(ctx, machine));
                return;
            }
        }
        else returnGate = 0f;

        // 1순위 : 던진 오브젝트 감지
        if (ctx.CanHearThrowObject(ctx.data.sightDistance, out var noisePos))
        {
            ctx.agent.speed = ctx.data.detectSpeed;
            ctx.agent.SetDestination(noisePos);
            ctx.anim.Play("Walk");
            return;
        }
        // 2순위 : 드롭 아이템과 상호작용
        if (ctx.CanSeeObject(ctx.data.sightDistance))
        {
            machine.ChangeState(new MonsterSpecialState(ctx, machine));
            return;
        }

        // 3 순위 : 플레이어 청각(“감지” 아님, 수사용 보정)
        hearTimer += Time.deltaTime;
        if (ctx.data.isaggressive && hearTimer >= hearInterval)
        {
            hearTimer -= hearInterval;
            if (ctx.CanHearPlayer(ctx.data.hearRange))
            {
                targetPos = ctx.player.position;
                ctx.agent.SetDestination(targetPos);
                chaseTimer = chaseTimeout;
            }
        }

        bool seeNow = ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle);
        if (seeNow && ctx.player) ctx.agent.SetDestination(ctx.player.position);

        // 목적지 도착했고 현재 시야가 아니면 수색-배회로
        if (!ctx.agent.pathPending && ctx.agent.remainingDistance <= ctx.agent.stoppingDistance)
        {
            if (!seeNow)
            {
                machine.ChangeState(new MonsterSearchWanderState(ctx, machine));
                return;
            }
        }

    }
    public void Exit() 
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        if (ctx.alert) ctx.alert.gameObject.SetActive(false);
    }
    // ========== 내부 로직 ==========
    bool SeePredicate()
    => ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle);
    async UniTaskVoid StartAggroWatcherAsync(CancellationToken token)
    {
        // 초기 아이콘: 물음표 + 시작색(흰색)
        if (ctx.alert && ctx.data.questionSprite)
        {
            ctx.alert.sprite = ctx.data.questionSprite;
            ctx.alert.color = ctx.data.questionStartColor; // white
            ctx.alert.gameObject.SetActive(true);
        }

        void Progress(float t)
        {
            if (!ctx.alert) return;
            // t는 0~1: '시야 true'가 이어진 시간 / 요구 시간
            ctx.alert.color = Color.Lerp(ctx.data.questionStartColor,
                                         ctx.data.questionEndColor, // red
                                         t);
        }

        // 시야만 2초 연속 유지돼야 true
        bool ok = await ConditionAwaiter.HoldTrueContinuously(
            ctx.data.aggroHoldSeconds,
            SeePredicate,       // 시야만
            Progress,
            token);

        if (token.IsCancellationRequested || !ok) return;

        // 전투 진입 직전, 느낌표 한 번만
        await ShowExclamationAsync(token);

        float delay = Mathf.Max(0f, ctx.data.preTransitionDelay);
        if (delay > 0f)
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

        if (token.IsCancellationRequested) return;

        if (ctx.data.istracing)
        {
            machine.ChangeState(new MonsterTraceState(ctx, machine));
        }
        // 전투몹인지 비전투몹인지에 따라 분기
        if (!token.IsCancellationRequested) 
        {
            if (ctx.isaggressive)
            {
                ctx.isCombat = true;
                machine.ChangeState(new CombatSuperState(ctx, machine));
            }
            else
            {
                machine.ChangeState(new MonsterFleeState(ctx, machine));
            }
        }
    }

    async UniTask ShowExclamationAsync(CancellationToken token)
    {
        if (!ctx.alert || !ctx.data.exclamationSprite) return;

        ctx.alert.sprite = ctx.data.exclamationSprite;
        ctx.alert.color = Color.red;
        ctx.alert.gameObject.SetActive(true);

        // 잠깐(0.5초) 보여주고 유지하거나, Combat 쪽에서 관리해도 됨
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
    }
}
public sealed class MonsterSearchWanderState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    const float searchTime = 5f;               // 5초 배회
    const float stuckTimeout = 1f;   // 한 지점에서 막힘 판정
    const float sampleCycle = 1f;   // 정상 도착 후 재샘플 주기

    float elapsed;
    float localTimer;
    float detectGate;

    public MonsterSearchWanderState(MonsterContext c, MonsterStateMachine m)
    { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.SearchWander);
        ctx.houndanimPlayer?.SetTag(MonsterStateTag.SearchWander, ctx);
        ctx.cleaneranimPlayer?.SetTag(MonsterStateTag.SearchWander, ctx);
        if (!ctx.data.canMove) { machine.ChangeState(new MonsterReturnState(ctx, machine)); return; }

        ctx.agent.speed = ctx.data.detectSpeed;   // Idle 보다 약간 빠름
        ctx.anim.Play("Walk");
        detectGate = 0f;
        PickRandomDest();
    }

    public void Tick()
    {
        elapsed += Time.deltaTime;
        localTimer += Time.deltaTime;

        // 감지 우선 처리: 투척 > 아이템 > 플레이어
        bool sensedNoise = ctx.CanHearThrowObject(ctx.data.sightDistance, out var noisePos);
        Transform itemTf = ctx.CanSeeObject(ctx.data.sightDistance);
        bool sensedPlayer = ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle)
                         || ctx.CanHearPlayer(ctx.data.hearRange);

        if (sensedNoise || itemTf || sensedPlayer)
        {
            detectGate += Time.deltaTime;
            if (detectGate >= ctx.data.detectGateDelay)
            {
                if (sensedNoise)
                {
                    machine.ChangeState(new MonsterDetectState(ctx, machine));
                    return;
                }
                else if (itemTf)
                {
                    machine.ChangeState(new MonsterSpecialState(ctx, machine));
                    return;
                }
                else // player
                {
                    machine.ChangeState(new MonsterDetectState(ctx, machine));
                    return;
                }
            }
        }
        else
        {
            detectGate = 0f;
        }

        if (elapsed >= searchTime)
        {
            machine.ChangeState(new MonsterReturnState(ctx, machine));
            return;
        }
        /* 주기적으로 새 배회 지점 */
        if (!ctx.agent.pathPending &&
            ctx.agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            PickRandomDest();
            return;
        }
        bool arrived = !ctx.agent.pathPending &&
                       ctx.agent.remainingDistance <= ctx.agent.stoppingDistance;

        if (arrived || localTimer >= stuckTimeout)
        {
            PickRandomDest();
            return;
        }
    }
    public void Exit() { }

    void PickRandomDest()
    {
        localTimer = 0f;
        Vector2 rnd = UnityEngine.Random.insideUnitCircle * ctx.data.wanderRadius;
        Vector3 dest = ctx.transform.position + (Vector3)rnd;
        ctx.agent.SetDestination(dest);
    }
}
sealed class MonsterReturnState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;
    float detectGate;
    bool ReturnLock;
    public MonsterReturnState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Return);
        ctx.houndanimPlayer?.SetTag(MonsterStateTag.Return, ctx);
        ctx.cleaneranimPlayer?.SetTag(MonsterStateTag.Return, ctx);
        ReturnLock = ctx.IsFastReturn;

        ctx.agent.isStopped = false;
        ctx.agent.speed = ctx.IsFastReturn ? ctx.data.fleeSpeed : ctx.data.detectSpeed;
        ctx.agent.SetDestination(ctx.spawner);
        ctx.anim.Play("Walk");
        detectGate = 0f;
    }

    public void Tick()
    {
        float dist = Vector2.Distance(ctx.transform.position, ctx.spawner);

        // FastReturn 모드 동안은 감지 완전 무시 → 스포너까지 무조건 복귀
        if (dist <= ctx.data.nearSpawnerDist)
        {
            machine.ChangeState(new MonsterIdleState(ctx, machine));
            return;
        }
        if (!ctx.agent.pathPending && ctx.agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            ctx.agent.SetDestination(ctx.spawner);
        }
        if (ReturnLock)
        {
            return; // 도중 Detect/Special 등으로 절대 전환하지 않음
        }

        // 이후는 평범한 귀환 로직
        bool isNear = dist <= ctx.data.nearSpawnerDist;
        bool isFar = dist > ctx.data.maxSpawnerDist;
        bool inMid = !isNear && !isFar;

        // 스포너 근접 → Idle 복귀
        if (isNear)
        {
            machine.ChangeState(new MonsterIdleState(ctx, machine));
            return;
        }

        if (isFar)
        {
            detectGate = 0f; // 게이트 누적도 리셋해서 우발 전환 방지
            return;          // 계속 스포너로 향하게 둠
        }

        bool sensedNoise = ctx.CanHearThrowObject(ctx.data.sightDistance, out var _);
        bool sensedPlayer = ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle)
                          || ctx.CanHearPlayer(ctx.data.hearRange);

        if (inMid && (sensedNoise || sensedPlayer))
        {
            detectGate += Time.deltaTime;
            if (detectGate >= ctx.data.detectGateDelay)
            {
                machine.ChangeState(new MonsterDetectState(ctx, machine));
                return;
            }
        }
        else
        {
            detectGate = 0f;
        }
    }

    public void Exit() { ctx.IsFastReturn = false; }
}
public sealed class MonsterStunState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;
    readonly bool goToFleeOnEnd;
    float duration;
    float elapsed;

    // 동일 스턴 중 갱신을 위해 노출
    public void Refresh(float addDuration, bool overrideToLonger = true)
    {
        if (overrideToLonger) duration = Mathf.Max(duration - elapsed, addDuration) + elapsed;
        else duration += addDuration;
    }

    public MonsterStunState(MonsterContext c, MonsterStateMachine m, float stunSec, bool goToFleeOnEnd = false)
    { ctx = c; machine = m; duration = Mathf.Max(0.01f, stunSec); this.goToFleeOnEnd = goToFleeOnEnd; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Stun);
        ctx.houndanimPlayer?.SetTag(MonsterStateTag.Stun, ctx);
        ctx.cleaneranimPlayer?.SetTag(MonsterStateTag.Stun, ctx);
        elapsed = 0f;

        // 이동 완전 정지
        ctx.SafeStopAgent();

        // 경고 아이콘 숨김
        if (ctx.alert) ctx.alert.gameObject.SetActive(false);

        // 스턴 애니 (없으면 Hit로 폴백)
        if (ctx.anim)
        {
            var hasStun = ctx.data.animator ? true : true;
            ctx.anim.Play("Stun", 0, 0f);
        }
        if (ctx.mono.TryGetComponent(out MonsterStomach stomach) && stomach.HasItems)
        {
            stomach.VomitAll(); // 또는 stomach.VomitOne();
        }
    }

    public void Tick()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            if (goToFleeOnEnd)
                machine.ChangeState(new MonsterFleeState(ctx, machine)); // Flee
            else
                machine.PopState(); // 스턴 끝 → 아래 상태 재개
        }
    }

    public void Exit()
    {
        // 이동 재개
        ctx.SafeResumeAgent();
    }
}
// AutoDestroy.cs
public class AutoDestroy : MonoBehaviour
{
    float life;
    public void Init(float sec) => life = sec;
    void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}