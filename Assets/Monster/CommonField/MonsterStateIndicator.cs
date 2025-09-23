using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterStateTag
{
    None,
    Idle, Detect, Flee, Special, Trace, Return, Stun, SearchWander, Killed,
    CombatMove, CombatAttack, SpinAttack
}

/// 몬스터 머리 위에 상태 아이콘을 띄우는 전담 컴포넌트
/// - 같은 GameObject(혹은 자식)에 붙이고, SpriteRenderer를 할당하세요.
/// - 각 상태별 스프라이트를 인스펙터에서 지정.
public sealed class MonsterStateIndicator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] SpriteRenderer spriteRenderer;   // 머리 위 아이콘용 SR
    [SerializeField] Vector3 worldOffset = new(0, 1.2f, 0);

    [Header("Sprites")]
    public Sprite idle;
    public Sprite detect;
    public Sprite flee;
    public Sprite special;
    public Sprite trace;
    public Sprite ret;               // Return
    public Sprite stun;
    public Sprite searchWander;
    public Sprite killed;
    public Sprite combatMove;
    public Sprite combatAttack;

    [Header("FX (optional)")]
    public bool popOnChange = true;
    public float popScale = 1.15f;
    public float popTime = 0.08f;

    Transform _owner;
    MonsterStateTag _current = MonsterStateTag.None;
    Vector3 _defaultLocalScale;

    void Awake()
    {
        _owner = transform.parent ? transform.parent : transform;

        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true); // ★ 자식까지 탐색

        if (spriteRenderer) _defaultLocalScale = spriteRenderer.transform.localScale;
        Hide();
    }

    void LateUpdate()
    {
        // 소폭 위치 고정(부모 기준 오프셋)
        if (_owner && spriteRenderer)
            spriteRenderer.transform.position = _owner.position + worldOffset;
    }

    public void Show(MonsterStateTag tag)
    {
        if (!spriteRenderer) return;

        // 동일 상태 재요청이면 스킵
        if (_current == tag && spriteRenderer.enabled) return;

        spriteRenderer.sprite = Resolve(tag);
        spriteRenderer.enabled = spriteRenderer.sprite != null;
        _current = tag;

        if (popOnChange && spriteRenderer.enabled)
            StopAllCoroutines(); StartCoroutine(PopOnce());
    }

    public void ShowCombatMove() => Show(MonsterStateTag.CombatMove);
    public void ShowCombatAttack() => Show(MonsterStateTag.CombatAttack);

    public void Hide()
    {
        if (!spriteRenderer) return;
        spriteRenderer.enabled = false;
        _current = MonsterStateTag.None;
    }

    Sprite Resolve(MonsterStateTag tag) => tag switch
    {
        MonsterStateTag.Idle => idle,
        MonsterStateTag.Detect => detect,
        MonsterStateTag.Flee => flee,
        MonsterStateTag.Special => special,
        MonsterStateTag.Trace => trace,
        MonsterStateTag.Return => ret,
        MonsterStateTag.Stun => stun,
        MonsterStateTag.SearchWander => searchWander,
        MonsterStateTag.Killed => killed,
        MonsterStateTag.CombatMove => combatMove,
        MonsterStateTag.CombatAttack => combatAttack,
        _ => null
    };
    void OnValidate()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }
    System.Collections.IEnumerator PopOnce()
    {
        if (!spriteRenderer) yield break;

        var t = 0f;
        var tr = spriteRenderer.transform;
        var start = _defaultLocalScale;
        var target = start * popScale;

        // scale up
        while (t < popTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popTime);
            tr.localScale = Vector3.Lerp(start, target, k);
            yield return null;
        }
        // back
        t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popTime);
            tr.localScale = Vector3.Lerp(target, start, k);
            yield return null;
        }
        tr.localScale = start;
    }
}