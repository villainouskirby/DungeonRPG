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
    public string attackKey = "attack";
    public string backstepKey = "backstep";


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
            case MonsterStateTag.CombatMove: return runKey;
            case MonsterStateTag.CombatAttack: return attackKey;
            case MonsterStateTag.BackStep: return backstepKey;
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

        float angleFromUp = Mathf.Atan2(fwd.x, fwd.y) * Mathf.Rad2Deg;
        if (angleFromUp < 0f) angleFromUp += 360f;

        const float UP_HALF = 0f;      // 위쪽 40° 구간(±20°)
        const float DOWN_CTR = 180f;    // 아래 중심
        const float DOWN_HALF = 0f;    // 아래 40° 구간(±20°)

        bool isUp =
            (angleFromUp <= UP_HALF) || (angleFromUp >= 360f - UP_HALF); // [-20°,20°]에 해당
        bool isDown =
            (Mathf.Abs(Mathf.DeltaAngle(angleFromUp, DOWN_CTR)) <= DOWN_HALF); // [160°,200°]

        if (isUp)
            return backKey;
        if (isDown)
            return frontKey;
        sideSignX = (fwd.x >= 0f) ? +1 : -1;
        return sideKey;
    }
}