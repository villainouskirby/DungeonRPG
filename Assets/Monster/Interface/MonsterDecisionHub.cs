using System;
using UnityEngine;

public sealed class MonsterDecisionHub
{
    readonly MonsterContext ctx;

    float _detectGate;
    float _returnGate;
    float _disengageHold;

    public Vector3 LastNoisePos { get; private set; }
    public Transform LastSeenItem { get; private set; }
    public bool IsFastReturnRequested { get; private set; }

    public MonsterDecisionHub(MonsterContext ctx) { this.ctx = ctx; ResetAll(); }

    public void ResetAll()
    {
        _detectGate = _returnGate = _disengageHold = 0f;
        LastNoisePos = Vector3.zero;
        LastSeenItem = null;
        IsFastReturnRequested = false;
    }

    public Route Decide(float dt)
    {
        // 0) 스포너 거리 → Return 게이트
        float dSpawn = Vector2.Distance(ctx.transform.position, ctx.spawner);
        if (dSpawn > ctx.data.maxSpawnerDist)
        {
            _returnGate += dt;
            if (_returnGate >= ctx.data.returnGateDelay)
            {
                IsFastReturnRequested = true;
                return Route.Return;
            }
        }
        else { _returnGate = 0f; IsFastReturnRequested = false; }

        // 1) 던진 오브젝트 소리 → Detect
        if (ctx.CanHearThrowObject(ctx.data.sightDistance, out var noisePos))
        {
            LastNoisePos = noisePos;
            if (Gate(ref _detectGate, dt, ctx.data.detectGateDelay))
                return Route.Detect;
            return Route.None;
        }

        // 2) 관심 아이템 → Special
        var itemTf = ctx.CanSeeObject(ctx.data.sightDistance);
        if (itemTf)
        {
            LastSeenItem = itemTf;
            if (Gate(ref _detectGate, dt, ctx.data.detectGateDelay))
                return Route.Special;
            return Route.None;
        }

        // 3a) 플레이어 청각 → Detect (감지 아님)
        if (ctx.CanHearPlayer(ctx.data.hearRange))
        {
            if (Gate(ref _detectGate, dt, ctx.data.detectGateDelay))
                return Route.Detect;
            return Route.None;
        }

        // 3b) 플레이어 시야 → Detect (Trace/Attack 승격은 DetectState에서 처리)
        if (ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle))
        {
            if (Gate(ref _detectGate, dt, ctx.data.detectGateDelay))
                return Route.Detect;
            return Route.None;
        }

        return Route.None; // Idle 유지
    }

    // 전투 해제(Combat 전용)
    public Route DecideDuringCombat(float dt)
    {
        // 스포너로부터 멀어짐 : 전투 중에는 즉시 귀환(게이트 X)
        float dSpawn = Vector2.Distance(ctx.transform.position, ctx.spawner);
        if (dSpawn > ctx.data.maxSpawnerDist)
        {
            IsFastReturnRequested = true;
            ctx.isCombat = false;          // 전투 종료
            return Route.Return;
        }

        // 전투 유지/해제 홀드
        bool see = ctx.CanSeePlayer(ctx.data.sightDistance, ctx.data.sightAngle);
        bool hear = ctx.CanHearPlayer(ctx.data.hearRange);
        float dPlayer = ctx.player ? Vector2.Distance(ctx.transform.position, ctx.player.position) : Mathf.Infinity;

        if (see || hear) _disengageHold = 0f;
        else _disengageHold += dt;

        // 플레이어가 충분히 멀어졌고 일정 시간 미감지 → 전투 종료 → 수사(Detect)로 복귀
        if (dPlayer > ctx.data.lostDistance && _disengageHold >= ctx.data.disengageHoldSeconds)
        {
            ctx.isCombat = false;
            return Route.Detect;
        }

        return Route.None; // 전투 유지
    }
    static bool Gate(ref float acc, float dt, float need)
    {
        acc += dt;
        if (acc >= need) { acc = 0f; return true; }
        return false;
    }
}