using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterTraceState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    float returnGate;

    public MonsterTraceState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Trace);
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

        if (ctx.CanSeeObject(ctx.data.sightDistance))
        {
            machine.ChangeState(new MonsterSpecialState(ctx, machine));
            return;
        }
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
