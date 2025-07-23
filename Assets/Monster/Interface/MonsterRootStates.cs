using System.Collections;
using System.Collections.Generic;
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
        restTimer = Random.Range(0.5f, 2f);
    }

    public void Tick()
    {
        // 플레이어 감지
        if (ctx.CanSeePlayer(ctx.data.sightDistance) || ctx.CanHearPlayer(ctx.data.hearRange))
        {
            machine.ChangeState(new MonsterDetectState(ctx, machine));
            return;
        }

        restTimer -= Time.deltaTime;
        if (restTimer > 0f) return;

        // 배회 목적지 선정
        Vector2 rnd = Random.insideUnitCircle * ctx.data.wanderRadius;
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
        if (ctx.CanSeePlayer(ctx.data.sightDistance) || ctx.CanHearPlayer(ctx.data.hearRange))
            machine.ChangeState(new MonsterDetectState(ctx, machine));
    }
    public void Exit() { }
}

// DetectState
public sealed class MonsterDetectState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

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
    }
    public void Tick()
    {
        // 스포너 한계 검사
        if (ctx.transform && ctx.spawner &&
            Vector2.Distance(ctx.transform.position, ctx.spawner.position) >= ctx.data.maxSpawnerDist)
        {
            ctx.IsFastReturn = true;
            machine.ChangeState(new MonsterReturnState(ctx, machine));
            return;
        }

        // 0.5초 주기로 소리 재탐색
        hearTimer += Time.deltaTime;
        if (hearTimer >= hearInterval)
        {
            hearTimer -= hearInterval;

            if (ctx.CanHearPlayer(ctx.data.hearRange))
            {
                targetPos = ctx.player.position;   // 새 소리 위치
                ctx.agent.SetDestination(targetPos);
                chaseTimer = chaseTimeout;          // 5초 리셋
            }
        }

        // 시야로 보이면 Combat/Flee 즉시 전환
        if (ctx.CanSeePlayer(ctx.data.sightDistance))
        {
            Debug.Log($"{ctx.data.monsterName} ▶ Combat 진입 조건 충족 (dist {Vector2.Distance(ctx.transform.position, ctx.player.position):F1})");
            machine.ChangeState(ctx.data.isaggressive
                               ? new CombatSuperState(ctx, machine)
                               : new MonsterFleeState(ctx, machine));
            return;
        }

        // 추적 타임아웃
        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0f)
        {
            bool nearSpawner = ctx.spawner &&
                               Vector2.Distance(ctx.transform.position, ctx.spawner.position)
                               <= ctx.data.nearSpawnerDist;

            machine.ChangeState(nearSpawner
                               ? new MonsterIdleState(ctx, machine)
                               : new MonsterReturnState(ctx, machine));
        }
    }
    public void Exit() { }
}
sealed class MonsterReturnState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    public MonsterReturnState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.agent.speed = ctx.IsFastReturn ? ctx.data.fleeSpeed : ctx.data.detectSpeed;
        ctx.agent.SetDestination(ctx.spawner.position);
        ctx.anim.Play("Walk");
    }

    public void Tick()
    {
        if (Vector2.Distance(ctx.transform.position, ctx.spawner.position) <= ctx.data.nearSpawnerDist)
            machine.ChangeState(new MonsterIdleState(ctx, machine));
    }

    public void Exit() { ctx.IsFastReturn = false; }
}
public sealed class MonsterKilledState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine root;

    public MonsterKilledState(MonsterContext c, MonsterStateMachine m)
    { ctx = c; root = m; }

    public void Enter()
    {
        ctx.agent.isStopped = true;
        ctx.anim.Play("Die");
        ctx.sr.color = Color.gray;
        ctx.transform.gameObject.layer = LayerMask.NameToLayer("Corpse"); // 선택
        ctx.transform.GetComponent<Collider2D>().enabled = false;

        // 2초 뒤 파괴
        ctx.transform.gameObject
            .AddComponent<AutoDestroy>()
            .Init(2f);           // 짧은 헬퍼 스크립트
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