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
        // 1순위 : 스포너 거리 초과 → Return
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

        // 2순위 : 던진 오브젝트 감지 (조금 지연하면서 변경)
        if (ctx.CanHearThrowObject(ctx.data.sightDistance, out var noisePos))
        {
            detectGate += Time.deltaTime;
            if (detectGate >= ctx.data.detectGateDelay)
            {
                machine.ChangeState(new MonsterDetectState(ctx, machine));
                return;
            }
        }
        else detectGate = 0f;
        // 3순위 : 아이템 감지 (아이템 감지하는 몬스터만), (이것도 일단 지연)
        if (ctx.CanSeeObject(ctx.data.sightDistance))
        {
            detectGate += Time.deltaTime;
            if (detectGate >= ctx.data.detectGateDelay)
            {
                machine.ChangeState(new MonsterSpecialState(ctx, machine));
                return;
            }
            return;
        }
        else detectGate = 0f;
        // 4순위 : 플레이어 감지 (조금 지연하면서 변경)
        if (ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle) || ctx.CanHearPlayer(ctx.data.hearRange))
        {
            detectGate += Time.deltaTime;
            if (detectGate >= ctx.data.detectGateDelay)
            {
                machine.ChangeState(new MonsterDetectState(ctx, machine));
                return;
            }
        }
        else detectGate = 0f;

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
    public void Exit() { }

    bool CanSee() => ctx.player && Vector2.Distance(ctx.transform.position, ctx.player.position) <= ctx.data.sightDistance;
    bool CanHear() => ctx.player && Vector2.Distance(ctx.transform.position, ctx.player.position) <= ctx.data.hearRange;
}

// WanderState : 이동만 담당, 도착하면 Idle 로 복귀
public sealed class MonsterWanderState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float timeout = 1f;
    float detectGate;

    public MonsterWanderState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter() {
        if (!ctx.data.canMove)         // 고정형 Idle 유지
            return;
        detectGate = 0f;
    }
    public void Tick()
    {
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

        if (ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle)|| ctx.CanHearPlayer(ctx.data.hearRange))
        {
            detectGate += Time.deltaTime;
            if (detectGate >= ctx.data.detectGateDelay)
            {
                machine.ChangeState(new MonsterDetectState(ctx, machine));
                return;
            }
        }
        else detectGate = 0f;
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
        ctx.anim.Play("Walk");
        ctx.agent.speed = ctx.data.detectSpeed;

        targetPos = ctx.player.position;      // 최초 소리 위치
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

        /* ─── 플레이어 청각 재탐색 (적대일 때만) ─── */
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
        /* ─── 플레이어 시야 감지 ─── */
        if (seeNow)
        {
            // 감시 태스크(ConditionAwaiter.HoldTrueContinuously)가 전환을 담당.
            // 플레이어 쪽으로 움직이기 유지.
            if (ctx.player)
            {
                ctx.agent.SetDestination(ctx.player.position);
            }
        }

        if (!ctx.agent.pathPending && ctx.agent.remainingDistance <= ctx.agent.stoppingDistance)
        {
            /* 플레이어를 못 봤다면 Search-Wander 로 */
            if (!ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle))
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
public sealed class MonsterTraceState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float returnGate;

    public MonsterTraceState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        returnGate = 0f;

        float spd = (ctx.data.traceSpeed > 0f) ? ctx.data.traceSpeed : ctx.data.detectSpeed;
        ctx.agent.speed = spd;
        ctx.agent.isStopped = false;
        ctx.anim.Play("Walk");
    }

    public void Tick()
    {
        // 플레이어 유효성
        if (!ctx.player)
        {
            machine.ChangeState(new MonsterSearchWanderState(ctx, machine));
            return;
        }

        // 스포너 거리 초과 → 귀환 게이트
        float distSpawn = Vector2.Distance(ctx.transform.position, ctx.spawner);
        if (distSpawn > ctx.data.maxSpawnerDist)
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

        // 추적 거리 유지 로직
        Vector3 p = ctx.player.position;
        Vector3 me = ctx.transform.position;
        Vector3 toPlayer = (p - me);
        float d = toPlayer.magnitude;

        float near = ctx.data.traceNearDistance;
        float far = ctx.data.traceFarDistance;
        float desired = ctx.data.traceDesiredDistance;

        if (d > far)                 // 너무 멀다 → 다가가기
        {
            Vector3 target = p - toPlayer.normalized * desired;
            if (ctx.TrySetDestinationSafe(target, 3f))
            {
                ctx.agent.isStopped = false;
                ctx.anim.Play("Walk");
            }
        }
        else if (d < near)           // 너무 가깝다 → 살짝 벌리기
        {
            // 플레이어 반대 방향으로 desired 링에 위치
            Vector3 target = me - toPlayer.normalized * (near - d + 0.5f);
            // 또는 player 기준 링: p + (-dir)*desired;
            if (ctx.TrySetDestinationSafe(target, 3f))
            {
                ctx.agent.isStopped = false;
                ctx.anim.Play("Walk");
            }
        }
        else                         // 적정 밴드 안 → 정지/대기
        {
            if (!ctx.agent.isStopped)
            {
                ctx.agent.isStopped = true;
                ctx.agent.velocity = Vector3.zero;
                ctx.anim.Play("Idle");
            }
            // 바라보는 방향만 유지하고 끝
        }
    }
    public void Exit()
    {
        ctx.agent.isStopped = false;
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
public sealed class MonsterKilledState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterController ctr;
    readonly MonsterStateMachine root;
    readonly GameObject go;
    Sprite sprite;
    string dt;
    public Monster_Info_Monster_DropTable dropData;
    public Dictionary<string, Item_Info_Item> ItemDic;
    public Dictionary<string, Monster_Info_Monster_DropTable> monsterDropDic;
    public MonsterKilledState(MonsterContext c, MonsterStateMachine m, GameObject go, MonsterController mc)
    { ctx = c; root = m; this.go = go; ctr = mc; }

    public void Enter()
    {
        ctx.agent.isStopped = true;
        ctx.anim.Play("Die");


        // 드롭테이블 로드
        dt = ctr.mdata.Monster_DT;
        monsterDropDic = SheetDataUtil.DicByKey(Monster_Info.Monster_DropTable, x => x.DropTable_id);
        if (!monsterDropDic.TryGetValue(dt, out dropData))
        {
            Debug.LogWarning($"[MonsterKilledState] DropTable not found: {dt}");
            Cleanup();
            return;
        }

        // 드랍 테이블 파싱
        var drops = DropTableResolver.RollDrops(dropData.DropTable_Info);
        ItemDic = SheetDataUtil.DicByKey(Item_Info.Item, x => x.id);

        // 인벤토리에 추가
        foreach (var d in drops)
        {
            if (!ItemDic.TryGetValue(d.itemId, out var itemInfo))
            {
                Debug.LogWarning($"[MonsterKilledState] Unknown item id: {d.itemId}");
                continue;
            }

            for (int i = 0; i < d.count; i++)
            {
                ThrowItemData itemData = new(ItemDic["ITM_MIN_ROC"], sprite, "PAR_MIN_ROC");
                UIPopUpHandler.Instance.InventoryScript.AddItem(itemData);
            }
        }

        Cleanup();
    }
    void Cleanup()
    {
        SpawnerPool.Instance.MonsterPool.Release(ctx.id, go);
    }
    public void Tick() { }
    public void Exit() { }
}
public sealed class MonsterStunState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float duration;
    float elapsed;

    // 동일 스턴 중 갱신을 위해 노출
    public void Refresh(float addDuration, bool overrideToLonger = true)
    {
        if (overrideToLonger) duration = Mathf.Max(duration - elapsed, addDuration) + elapsed;
        else duration += addDuration;
    }

    public MonsterStunState(MonsterContext c, MonsterStateMachine m, float stunSec)
    { ctx = c; machine = m; duration = Mathf.Max(0.01f, stunSec); }

    public void Enter()
    {
        elapsed = 0f;

        // 이동 완전 정지
        ctx.agent.isStopped = true;
        ctx.agent.velocity = Vector3.zero;

        // 경고 아이콘 숨김
        if (ctx.alert) ctx.alert.gameObject.SetActive(false);

        // 스턴 애니 (없으면 Hit로 폴백)
        if (ctx.anim)
        {
            var hasStun = ctx.data.animator ? true : true;
            ctx.anim.Play("Stun", 0, 0f);
        }
    }

    public void Tick()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            // 스턴 끝 → 아래 상태 재개
            machine.PopState();
        }
    }

    public void Exit()
    {
        // 이동 재개
        ctx.agent.isStopped = false;
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