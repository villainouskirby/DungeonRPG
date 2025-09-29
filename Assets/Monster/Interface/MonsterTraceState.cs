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
    // 이동 정지 감지용
    Vector3 _prevPos;
    float _stillAccum;                 // 정지 누적 시간
    MonsterStateTag _lastAnimTag = MonsterStateTag.None;

    const float MOVE_SPEED_EPS = 0.05f;  // m/s 이하이면 "멈춤"으로 간주
    const float STILL_HOLD_SEC = 0.25f;  // 이 시간 이상 멈춰있을 때 Idle 전환
    const float ANIM_UPDATE_COOLD = 0.05f;  // 과도한 SetTag 스팸 방지(옵션)
    float dt;
    float _animCooldown;
    public MonsterTraceState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Trace);
        ctx.animationHub?.SetTag(MonsterStateTag.Trace, ctx);
        returnGate = 0f;

        float spd = (ctx.data.traceSpeed > 0f) ? ctx.data.traceSpeed : ctx.data.detectSpeed;
        ctx.agent.speed = spd;
        ctx.agent.isStopped = false;

        _prevPos = ctx.transform.position;
        _stillAccum = 0f;
        _animCooldown = 0f;
    }

    public void Tick()
    {
        dt = Time.deltaTime;
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
            }
        }
        else                         // 적정 밴드 안 → 정지/대기
        {
            if (!ctx.agent.isStopped)
            {
                ctx.agent.isStopped = true;
                ctx.agent.velocity = Vector3.zero;
            }
            // 바라보는 방향만 유지하고 끝
        }
        UpdateLocomotionTag(dt);
    }
    public void Exit()
    {
        ctx.agent.isStopped = false;
    }
    void UpdateLocomotionTag(float dt)
    {
        _animCooldown -= dt;

        // 속도 추정: 위치 변화 / dt (NavMeshAgent.velocity가 더 불안정할 때가 있어 위치 기반)
        Vector3 curPos = ctx.transform.position;
        float speed = (curPos - _prevPos).magnitude / Mathf.Max(1e-5f, dt);
        _prevPos = curPos;

        bool moving = speed > MOVE_SPEED_EPS;

        if (moving) _stillAccum = 0f;
        else _stillAccum += dt;

        // Idle 전환 조건을 만족하면 Idle, 아니면 Trace로 유지
        MonsterStateTag desired = (!moving && _stillAccum >= STILL_HOLD_SEC)
                                ? MonsterStateTag.Idle
                                : MonsterStateTag.Trace;

        // 과도한 호출 방지 + 변화 있을 때만 SetTag
        if (desired != _lastAnimTag && _animCooldown <= 0f)
        {
            ctx.animationHub?.SetTag(desired, ctx);
            _lastAnimTag = desired;
            _animCooldown = ANIM_UPDATE_COOLD; // 스팸 방지
        }
    }
}
