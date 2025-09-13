using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 몬스터별(하운드용) 애니메이션 재생기.
/// - 인스펙터에서 Idle/Walk/Run에 대응하는 Animator State 이름(또는 Clip 이름)을 지정.
/// - 상태 전이 시 MonsterStateTag와 MonsterContext를 넘겨 PlayFor(...)만 호출하면 됨.
public sealed class HoundAnimationPlayer : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer; // side 좌/우 flipX에 사용

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

    /// 각 상태 Enter에서 한 줄로 호출
    public void SetTag(MonsterStateTag tag, MonsterContext ctx)
    {
        _tag = tag;
        _ctx = ctx;
        PlayNow(); // 즉시 갱신
    }

    void LateUpdate()
    {
        if (_tag != MonsterStateTag.None && _ctx != null)
            PlayNow(); // 방향 바뀌면 자동 갱신
    }

    void PlayNow()
    {
        if (!animator) return;

        var motion = ResolveMotion(_tag, _ctx);
        if (motion == null) return;

        // 방향 + side 좌/우 부호
        string dir = ResolveDirectionKey(_ctx, out int sideSignX);

        // side일 때만 좌/우를 flipX로 표현
        if (dir == sideKey && spriteRenderer)
            spriteRenderer.flipX = (sideSignX < 0);

        string stateName = $"{prefix}{dir}_{motion}";
        if (_currentStateName == stateName) return;

        animator.Play(stateName, 0, 0f);
        _currentStateName = stateName;
        // Debug.Log($"HoundAnim ▶ {stateName}");
    }

    string ResolveMotion(MonsterStateTag tag, MonsterContext ctx)
    {
        switch (tag)
        {
            case MonsterStateTag.Idle: return idleKey;
            case MonsterStateTag.Stun: return idleKey;
            case MonsterStateTag.Detect: return walkKey;
            case MonsterStateTag.SearchWander: return walkKey;
            case MonsterStateTag.Trace: return walkKey;
            case MonsterStateTag.Return: return (ctx != null && ctx.IsFastReturn) ? runKey : walkKey;
            case MonsterStateTag.CombatMove: return runKey;
            case MonsterStateTag.CombatAttack: return runKey;
            case MonsterStateTag.Flee: return runKey;
            // 그 외 상태는 애니 바꾸지 않음
            default: return null;
        }
    }

    // front/back/side 판정 + side의 좌/우 부호(+1/-1)
    string ResolveDirectionKey(MonsterContext ctx, out int sideSignX)
    {
        sideSignX = 0;
        Vector2 fwd = (ctx != null) ? ctx.GetForward() : Vector2.right;

        if (fwd.sqrMagnitude < 0.0001f)
            return sideKey; // 정지 시 기본 side (flipX 유지)

        if (Mathf.Abs(fwd.x) > Mathf.Abs(fwd.y))
        {
            sideSignX = (fwd.x >= 0f) ? +1 : -1;
            return sideKey; // 좌/우는 flipX로만 처리
        }
        else
        {
            return (fwd.y > 0f) ? backKey : frontKey;
        }
    }
}