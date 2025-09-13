using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

/// Cleaner 전용 애니 플레이어
/// - 기본은 방향X(Idle/Walk만으로도 동작).
/// - 나중에 방향이 필요하면 useDirectionOn 를 켜고
///   front/back/side 3방향 × idle/walk/run(옵션) 네이밍을 맞추면 됨.
public sealed class CleanerAnimationPlayer : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer; // 방향 사용 시 side 좌/우 flipX에만 필요

    [Header("Use Direction?")]
    public bool useDirectionOn = false; // false면 방향 무시: idle/walk(run→walk)만 사용

    [Header("Naming")]
    public string prefix = "Cleaner";  // 예: Cleaner_idle, Cleaner_walk
    public string idleKey = "idle";
    public string walkKey = "walk";
    public string runKey = "run";     // 없으면 자동으로 walk로 대체

    // 방향 키(옵션). useDirectionOn=true일 때만 사용
    public string frontKey = "front";
    public string backKey = "back";
    public string sideKey = "side";

    MonsterStateTag _tag = MonsterStateTag.None;
    MonsterContext _ctx;
    string _currentStateName;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }
#endif

    /// 각 상태 Enter()에서 호출
    public void SetTag(MonsterStateTag tag, MonsterContext ctx)
    {
        _tag = tag;
        _ctx = ctx;
        PlayNow();
    }

    void LateUpdate()
    {
        if (_tag != MonsterStateTag.None && _ctx != null)
            PlayNow();
    }

    void PlayNow()
    {
        if (!animator) return;

        // 어떤 모션(idle/walk/run)을 재생할지
        string motion = ResolveMotion(_tag, _ctx);
        if (motion == null) return;

        // 클립이 없는 run은 walk로 대체(Idle/Walk만 있는 세팅 지원)
        if (!HasState(S(motion)) && motion == runKey)
            motion = walkKey;

        string stateName;

        if (!useDirectionOn)
        {
            stateName = S(motion); // Cleaner_idle / Cleaner_walk / Cleaner_run(없으면 위에서 walk로)
        }
        else
        {
            // 방향 키 계산(front/back/side) – Cleaners later proof
            string dir = ResolveDirectionKey(_ctx, out int sideSignX);

            // side일 때 좌/우는 flipX로만 해결
            if (dir == sideKey && spriteRenderer)
                spriteRenderer.flipX = (sideSignX < 0);

            stateName = $"{prefix}{dir}_{motion}"; // 예: Cleanerfront_walk
            // 방향 클립이 없다면 방향 없이 재생(유연성)
            if (!HasState(stateName))
                stateName = S(motion);
        }

        if (_currentStateName == stateName) return;
        animator.Play(stateName, 0, 0f);
        _currentStateName = stateName;
        // Debug.Log($"CleanerAnim ▶ {stateName}");
    }

    string ResolveMotion(MonsterStateTag tag, MonsterContext ctx)
    {
        switch (tag)
        {
            case MonsterStateTag.Idle:
            case MonsterStateTag.Stun: return idleKey;
            case MonsterStateTag.Detect:
            case MonsterStateTag.SearchWander:
            case MonsterStateTag.Trace: return walkKey;
            case MonsterStateTag.Return: return (ctx != null && ctx.IsFastReturn) ? runKey : walkKey;
            case MonsterStateTag.CombatMove:
            case MonsterStateTag.CombatAttack:
            case MonsterStateTag.Flee: return runKey;
            default: return null; // Special/Killed 등은 유지
        }
    }

    // 방향(옵션) 계산
    string ResolveDirectionKey(MonsterContext ctx, out int sideSignX)
    {
        sideSignX = 0;
        if (ctx == null) return sideKey;

        Vector2 fwd = ctx.GetForward();
        if (fwd.sqrMagnitude < 0.0001f) return sideKey;

        if (Mathf.Abs(fwd.x) > Mathf.Abs(fwd.y))
        {
            sideSignX = (fwd.x >= 0) ? +1 : -1;
            return sideKey;
        }
        return (fwd.y > 0f) ? backKey : frontKey;
    }

    // 유틸
    string S(string motion) => $"{prefix}_{motion}";

    bool HasState(string stateName)
    {
        if (!animator) return false;
        // AnimatorController가 클립 이름으로 직접 Play를 지원하므로
        // 보수적으로 항상 true를 돌려도 동작하나,
        // 레이어0 기준 state hash로 빠르게 확인하려면 아래처럼:
        int hash = Animator.StringToHash(stateName);
        return animator.HasState(0, hash);
    }
}