using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class TitanAnimationPlayer : AnimationPlayerBase
{
    [Header("Targets")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer; // 선택

    [Header("Use Direction?")]
    public bool useDirectionOn = false;

    [Header("Naming")]
    public string prefix = "titan";
    public string idleKey = "idle";
    public string walkKey = "walk";
    public string runKey = "run";
    public string preSpinAttackKey = "preSpinAttack";
    public string spinAttackKey = "spinAttack";

    [Header("Direction Keys (optional)")]
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

    public override bool IsActiveFor(MonsterContext ctx) => true;

    public override void SetTag(MonsterStateTag tag, MonsterContext ctx)
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

        string motion = ResolveMotion(_tag, _ctx);
        if (motion == null) return;

        // run 미보유 → walk 대체
        if (!HasState(SetString(motion)) && motion == runKey)
            motion = walkKey;

        string stateName;
        if (!useDirectionOn)
        {
            stateName = SetString(motion);                 // Cleaner_idle / Cleaner_walk / Cleaner_run
        }
        else
        {
            string dir = ResolveDirectionKey(_ctx, out int sideSignX);
            if (dir == sideKey && spriteRenderer)
                spriteRenderer.flipX = (sideSignX < 0);

            stateName = $"{prefix}{dir}_{motion}"; // Cleanerfront_walk
            if (!HasState(stateName))
                stateName = SetString(motion);             // 방향 클립 없으면 무시
        }

        if (_currentStateName == stateName) return;
        animator.Play(stateName, 0, 0f);
        _currentStateName = stateName;
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
            case MonsterStateTag.PreSpinAttack: return preSpinAttackKey;
            case MonsterStateTag.SpinAttack: return spinAttackKey;
            default: return null;
        }
    }

    string ResolveDirectionKey(MonsterContext ctx, out int sideSignX)
    {
        sideSignX = 0;
        if (!useDirectionOn || ctx == null) return sideKey;

        Vector2 fwd = ctx.GetForward();
        if (fwd.sqrMagnitude < 0.0001f) return sideKey;

        if (Mathf.Abs(fwd.x) > Mathf.Abs(fwd.y))
        {
            sideSignX = (fwd.x >= 0) ? +1 : -1;
            return sideKey;
        }
        return (fwd.y > 0f) ? backKey : frontKey;
    }

    string SetString(string motion) => $"{prefix}_{motion}";

    bool HasState(string stateName)
    {
        if (!animator) return false;
        int hash = Animator.StringToHash(stateName);
        return animator.HasState(0, hash);
    }
}