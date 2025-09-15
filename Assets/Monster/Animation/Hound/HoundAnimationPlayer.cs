using UnityEngine;

public sealed class HoundAnimationPlayer : AnimationPlayerBase
{
    [Header("Targets")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer; // side 좌/우 flipX

    [Header("Naming")]
    public string prefix = "hound";
    public string frontKey = "front";
    public string backKey = "back";
    public string sideKey = "side";

    [Header("Motion Keys")]
    public string idleKey = "idle";
    public string walkKey = "walk";
    public string runKey = "run";

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

    public override bool IsActiveFor(MonsterContext ctx) => true; // 필요하면 필터링 로직 추가

    public override void SetTag(MonsterStateTag tag, MonsterContext ctx)
    {
        _tag = tag;
        _ctx = ctx;
        PlayNow();
    }

    void LateUpdate()
    {
        if (_tag != MonsterStateTag.None && _ctx != null)
            PlayNow(); // 방향 변화 자동 갱신
    }

    void PlayNow()
    {
        if (!animator) return;

        var motion = ResolveMotion(_tag, _ctx);
        if (motion == null) return;

        // 방향 계산 + side일 때 flipX
        string dir = ResolveDirectionKey(_ctx, out int sideSignX);
        if (dir == sideKey && spriteRenderer)
            spriteRenderer.flipX = (sideSignX < 0);

        string stateName = $"{prefix}{dir}_{motion}";
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
            default: return null; // Special/Killed 등은 유지
        }
    }

    string ResolveDirectionKey(MonsterContext ctx, out int sideSignX)
    {
        sideSignX = 0;
        Vector2 fwd = (ctx != null) ? ctx.GetForward() : Vector2.right;

        if (fwd.sqrMagnitude < 0.0001f)
            return sideKey;

        if (Mathf.Abs(fwd.x) > Mathf.Abs(fwd.y))
        {
            sideSignX = (fwd.x >= 0f) ? +1 : -1;
            return sideKey; // 좌우는 flipX로만
        }
        else
        {
            return (fwd.y > 0f) ? backKey : frontKey;
        }
    }
}