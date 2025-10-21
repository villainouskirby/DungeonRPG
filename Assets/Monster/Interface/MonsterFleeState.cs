using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public sealed class MonsterFleeState : IMonsterState
{
    readonly MonsterContext ctx;
    readonly MonsterStateMachine machine;

    // 이동 정지 감지용
    Vector3 _prevPos;
    float _stillAccum;
    MonsterStateTag _lastAnimTag = MonsterStateTag.None;

    const float MOVE_SPEED_EPS = 0.05f;    // m/s 이하 → 멈춤
    const float STILL_HOLD_SEC = 0.25f;    // 멈춤 누적 시간
    const float ANIM_UPDATE_COOLD = 0.05f; // SetTag 스팸 방지
    float _animCooldown;

    float _stateEnterTime;
    bool _isDashing;   // 돌진 중 여부
    bool _isReleased;  // 릴리즈 처리 여부
    float _elapsed;    // 도망 경과 시간

    bool cleanerMode;
    bool bettleMode;
    float cleanerWaitTimer;
    public MonsterFleeState(MonsterContext c, MonsterStateMachine m) { ctx = c; machine = m; }

    public void Enter()
    {
        ctx.indicator?.Show(MonsterStateTag.Flee);
        ctx.animationHub?.SetTag(MonsterStateTag.Flee, ctx);
        if (ctx.alert) ctx.alert.gameObject.SetActive(false);
        _stateEnterTime = Time.time;
        float spd = (ctx.data.fleeSpeed > 0f) ? ctx.data.fleeSpeed : ctx.data.detectSpeed;
        ctx.agent.speed = spd;
        ctx.agent.isStopped = false;

        bettleMode = (ctx.data.category == MonsterData.MonsterCategory.Beetle);
        cleanerMode = (ctx.data.category == MonsterData.MonsterCategory.Cleaner);
        if (bettleMode)
        {
            ctx.animationHub?.SetTag(MonsterStateTag.Flee, ctx);
        }
        if (cleanerMode)
        {
            cleanerWaitTimer = 0f;
            ctx.TrySetDestinationSafe(ctx.spawner, 3f);
            return;                                    // 일반 도망 로직 생략
        }

        _prevPos = ctx.transform.position;
        _stillAccum = 0f;
        _animCooldown = 0f;
        _isDashing = false;
        _isReleased = false;
        _elapsed = 0f;
    }

    public void Tick()
    {
        if (cleanerMode)
        {
            // 스포너로 계속 목적지 업데이트
            ctx.TrySetDestinationSafe(ctx.spawner, 3f);
            float dist = Vector2.Distance(ctx.transform.position, ctx.spawner);
            if (dist <= ctx.data.nearSpawnerDist + 0.05f)
            {
                // 도착 후 대기
                ctx.SafeStopAgent();
                ctx.animationHub?.SetTag(MonsterStateTag.Idle, ctx);
                cleanerWaitTimer += Time.deltaTime;
                if (cleanerWaitTimer >= 1f)
                {
                    // 여기서 사라지는 애니 후 지연을 추가하면 될듯
                    SpawnerPool.Instance.MonsterPool.Release(ctx.id, ctx.mono.gameObject);
                    return;
                }
            }
            return;  // Cleaner 이외에는 일반 도망 로직 건드리지 않음
        }
        if (_isReleased) return; // 이미 풀 반환된 경우 안전 가드
        if (_isDashing) return;  // 돌진 중에는 다른 로직 무시

        // 경과 시간 체크 → 10초 후 자동 소멸
        _elapsed += Time.deltaTime;
        if (_elapsed >= 0.6f)
        {
            ctx.animationHub?.SetTag(MonsterStateTag.SearchWander, ctx);
        }
        if (_elapsed >= 10f)
        {
            ReleaseAndCleanup();
            return;
        }

        // 플레이어 없으면 탐색으로 전환
        if (!ctx.player)
        {
            machine.ChangeState(new MonsterSearchWanderState(ctx, machine));
            return;
        }

        // 플레이어 거리 계산
        Vector3 p = ctx.player.position;
        Vector3 me = ctx.transform.position;
        Vector3 toPlayer = (p - me);
        float d = toPlayer.magnitude;

        float near = ctx.data.fleeNearDistance;

        if (d < near) // 너무 가까우면 → 반대방향 이동
        {
            Vector3 target = me - toPlayer.normalized * (near - d + 0.5f);
            if (ctx.TrySetDestinationSafe(target, 3f))
                ctx.agent.isStopped = false;
        }
        else // 충분히 벌어졌으면 멈춤
        {
            if (!ctx.agent.isStopped)
            {
                ctx.agent.isStopped = true;
                ctx.agent.velocity = Vector3.zero;
            }
        }

        // 이동 감지 & Idle 태그 전환 + 돌진 트리거
        UpdateLocomotionAndDash();
    }

    public void Exit()
    {
        ctx.agent.isStopped = false;
    }

    void UpdateLocomotionAndDash()
    {
        float dt = Time.deltaTime;
        _animCooldown -= dt;

        Vector3 curPos = ctx.transform.position;
        float speed = (curPos - _prevPos).magnitude / Mathf.Max(1e-5f, dt);
        _prevPos = curPos;

        bool moving = speed > MOVE_SPEED_EPS;
        if (moving) _stillAccum = 0f;
        else _stillAccum += dt;

        MonsterStateTag desired = (!moving && _stillAccum >= STILL_HOLD_SEC)
                                ? MonsterStateTag.Idle
                                : MonsterStateTag.Flee;

        if (desired != _lastAnimTag && _animCooldown <= 0f)
        {
            ctx.animationHub?.SetTag(desired, ctx);
            _lastAnimTag = desired;
            _animCooldown = ANIM_UPDATE_COOLD;
        }

        // 정지 상태가 일정 시간 지속되면 → 돌진 시작
        if (_stateEnterTime + 3f < Time.time &&!_isDashing && !moving && _stillAccum >= STILL_HOLD_SEC)
        {
            DashTowardPlayer().Forget();
        }
    }

    async UniTaskVoid DashTowardPlayer()
    {
        _isDashing = true;

        if (!ctx.player) { _isDashing = false; return; }

        Vector3 dir = (ctx.player.position - ctx.transform.position).normalized;
        float dashSpeed = ctx.data.fleeDashSpeed > 0 ? ctx.data.fleeDashSpeed : ctx.data.fleeSpeed * 1.5f;
        float duration = 2.4f;
        float elapsed = 0f;

        ctx.agent.enabled = false; // NavMeshAgent 끄고 수동 이동
        ctx.animationHub?.SetTag(MonsterStateTag.SearchWander, ctx);

        while (elapsed < duration)
        {
            ctx.transform.position += dir * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        ctx.agent.enabled = true;  // NavMeshAgent 복구
        ctx.agent.isStopped = true;
        _isDashing = false;
        _stillAccum = 0f;
    }

    void ReleaseAndCleanup()
    {
        if (_isReleased) return;
        _isReleased = true;

        ctx.agent.isStopped = true;
        ctx.agent.velocity = Vector3.zero;

        ctx.animationHub?.SetTag(MonsterStateTag.Hide, ctx);

        VanishAndRelease().Forget();
    }

    async UniTaskVoid VanishAndRelease()
    {
        await UniTask.Delay(1000);

        SpawnerPool.Instance.MonsterPool.Release(ctx.id, ctx.mono.gameObject);
    }
}