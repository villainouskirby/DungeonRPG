using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
public sealed class MonsterIdleState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float restTimer;

    public MonsterIdleState(MonsterContext c, MonsterStateMachine m)
    { ctx = c; machine = m; }

    public void Enter()
    {
        if (!ctx.data.canMove)         // 고정형 Idle 유지
            return;
        ctx.anim.Play("Idle");
        restTimer = UnityEngine.Random.Range(0.5f, 2f);
    }

    public void Tick()
    {
        // TODO : 현재는 그냥 순서대로 가중치를 둬서 로직을 변경하지만 가중치를 설정해 분기 필요
        if (ctx.CanHearThrowObject(ctx.data.sightDistance, out var noisePos))
        {
            machine.ChangeState(new MonsterDetectState(ctx, machine));
        }
        // 아이템 감지
        if (ctx.CanSeeObject(ctx.data.sightDistance))
        {
            machine.ChangeState(new MonsterSpecialState(ctx, machine));
            return;
        }

        // 플레이어 감지
        if (ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle) || ctx.CanHearPlayer(ctx.data.hearRange))
        {
            machine.ChangeState(new MonsterDetectState(ctx, machine));
            return;
        }

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

    public MonsterWanderState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter() {
        if (!ctx.data.canMove)         // 고정형 Idle 유지
            return;
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

        // 도중에 플레이어가 보이면 Detect
        if (ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle) || ctx.CanHearPlayer(ctx.data.hearRange))
            machine.ChangeState(new MonsterDetectState(ctx, machine));
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

    public MonsterDetectState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.anim.Play("Walk");
        ctx.agent.speed = ctx.data.detectSpeed;

        targetPos = ctx.player.position;      // 최초 소리 위치
        ctx.agent.SetDestination(targetPos);

        hearTimer = 0f;
        chaseTimer = chaseTimeout;

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

    public MonsterSearchWanderState(MonsterContext c, MonsterStateMachine m)
    { ctx = c; machine = m; }

    public void Enter()
    {
        if (!ctx.data.canMove) { machine.ChangeState(new MonsterReturnState(ctx, machine)); return; }

        ctx.agent.speed = ctx.data.detectSpeed;   // Idle 보다 약간 빠름
        ctx.anim.Play("Walk");
        PickRandomDest();
    }

    public void Tick()
    {
        elapsed += Time.deltaTime;
        localTimer += Time.deltaTime;
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

    public MonsterReturnState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.agent.speed = ctx.IsFastReturn ? ctx.data.fleeSpeed : ctx.data.detectSpeed;
        ctx.agent.SetDestination(ctx.spawner);
        ctx.anim.Play("Walk");
    }

    public void Tick()
    {
        if (Vector2.Distance(ctx.transform.position, ctx.spawner) <= ctx.data.nearSpawnerDist)
            machine.ChangeState(new MonsterIdleState(ctx, machine));
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