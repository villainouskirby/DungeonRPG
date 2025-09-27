using UnityEngine;

/// <summary>
/// Canvas 없이 SpriteRenderer만으로 "강공격 차징 동안" 표시되는 진행 게이지.
/// AttackController의 OnChargeStart/Progress/End 이벤트를 구독해 표시/업데이트/숨김.
/// </summary>
[ExecuteAlways]
public class PlayerChargeGuage : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;   // 기본: 플레이어 Transform (비면 자동 탐색)
    [SerializeField] private float yOffset = 1.6f;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer bg;
    [SerializeField] private SpriteRenderer fill;

    [Header("Visual")]
    [SerializeField] private float fullWidth = 1.6f;
    [SerializeField] private float height = 0.22f;
    [SerializeField] private Color fullColor = new Color(0.25f, 0.95f, 0.25f);
    [SerializeField] private Color lowColor = new Color(0.95f, 0.25f, 0.25f);

    [Tooltip("게이지가 1→0(남은시간)으로 줄어드는 방식이면 체크")]
    [SerializeField] private bool countDown = false;

    [Header("Sorting")]
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int orderInLayer = 120;

    float _ratio = 0f;   // 0~1
    bool _visible = false;

    AttackController _attack;

    void Reset()
    {
        if (bg) bg.drawMode = SpriteDrawMode.Sliced;
        if (fill) fill.drawMode = SpriteDrawMode.Sliced;
    }

    void Awake()
    {
        EnsureTargetAndAttack();
        SetupSorting();
        ApplySize(0f);
        ApplyColor(1f);
        SetVisible(false);
    }

    void OnEnable()
    {
        EnsureTargetAndAttack();
        TrySubscribe();
        UpdateTransform();
    }

    void OnDisable()
    {
        TryUnsubscribe();
    }

    void LateUpdate()
    {
        EnsureTargetAndAttack();
        UpdateTransform();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SetupSorting();
            ApplySize(_ratio);
            ApplyColor(_ratio);
            SetVisible(_visible);
        }
#endif
    }

    void EnsureTargetAndAttack()
    {
        if (target == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc) target = pc.transform;
        }
        if (_attack == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc) _attack = pc.GetComponent<AttackController>();
            if (_attack == null) _attack = FindObjectOfType<AttackController>();
        }
    }

    void TrySubscribe()
    {
        if (_attack == null) return;
        _attack.OnChargeStart += HandleChargeStart;
        _attack.OnChargeProgress += HandleChargeProgress;
        _attack.OnChargeEnd += HandleChargeEnd;
    }

    void TryUnsubscribe()
    {
        if (_attack == null) return;
        _attack.OnChargeStart -= HandleChargeStart;
        _attack.OnChargeProgress -= HandleChargeProgress;
        _attack.OnChargeEnd -= HandleChargeEnd;
    }

    // ===== Attack Events =====
    void HandleChargeStart(float duration)
    {
        _ratio = countDown ? 1f : 0f;
        ApplySize(_ratio);
        ApplyColor(_ratio);
        SetVisible(true);
    }

    void HandleChargeProgress(float elapsed, float duration, float ratio)
    {
        _ratio = Mathf.Clamp01(countDown ? (1f - ratio) : ratio);
        ApplySize(_ratio);
        ApplyColor(_ratio);
    }

    void HandleChargeEnd()
    {
        _ratio = 0f;
        ApplySize(_ratio);
        SetVisible(false);
    }

    // ===== Render Helpers =====
    void ApplySize(float ratio)
    {
        if (bg)
        {
            bg.drawMode = SpriteDrawMode.Sliced;
            bg.size = new Vector2(fullWidth, height);
        }

        if (fill)
        {
            fill.drawMode = SpriteDrawMode.Sliced;
            float w = Mathf.Max(0f, fullWidth * ratio);
            fill.size = new Vector2(w, height);

            // 왼쪽 기준 차오르도록 좌표 보정
            float leftEdge = -fullWidth * 0.5f;
            float centerX = leftEdge + (w * 0.5f);
            fill.transform.localPosition = new Vector3(centerX, 0f, 0f);
        }
    }

    void ApplyColor(float ratio)
    {
        if (!fill) return;
        Color c = Color.Lerp(lowColor, fullColor, ratio);
        var mpb = new MaterialPropertyBlock();
        fill.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", c);
        fill.SetPropertyBlock(mpb);
    }

    void SetupSorting()
    {
        if (bg)
        {
            bg.sortingLayerName = sortingLayer;
            bg.sortingOrder = orderInLayer;
        }
        if (fill)
        {
            fill.sortingLayerName = sortingLayer;
            fill.sortingOrder = orderInLayer + 1;
        }
    }

    void UpdateTransform()
    {
        if (!target) return;
        var p = target.position;
        transform.position = new Vector3(p.x, p.y + yOffset, p.z);
        transform.rotation = Quaternion.identity;
    }

    void SetVisible(bool on)
    {
        _visible = on;
        if (bg) bg.enabled = on;
        if (fill) fill.enabled = on;
    }
}
