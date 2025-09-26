using UnityEngine;

/// <summary>
/// Canvas 없이 SpriteRenderer만으로 "포션 마시는 동안" 표시되는 진행 게이지.
/// - fill은 drawMode=Sliced(or Tiled)에서 size.x를 변경해 길이를 조절.
/// - PotionManager의 OnGaugeStart/Progress/End 이벤트를 구독해 표시/숨김 및 진행률을 갱신.
/// </summary>
[ExecuteAlways]
public class PlayerPotionGuage : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;   // 기본: 플레이어 Transform (비면 자동 탐색)
    [SerializeField] private float yOffset = 1.2f;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer bg;
    [SerializeField] private SpriteRenderer fill;

    [Header("Visual")]
    [SerializeField] private float fullWidth = 1.5f;
    [SerializeField] private float height = 0.2f;
    [SerializeField] private Color fullColor = new Color(0.25f, 0.95f, 0.25f);
    [SerializeField] private Color lowColor = new Color(0.95f, 0.25f, 0.25f);

    [Tooltip("게이지가 1→0으로 줄어드는(남은시간) 방식이면 체크")]
    [SerializeField] private bool countDown = false;

    [Header("Sorting")]
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int orderInLayer = 100;

    // 내부 상태
    float _ratio = 0f;          // 0~1
    bool _visible = false;

    void Reset()
    {
        if (bg) bg.drawMode = SpriteDrawMode.Sliced;
        if (fill) fill.drawMode = SpriteDrawMode.Sliced;
    }

    void Awake()
    {
        EnsureTarget();
        SetupSorting();
        ApplySize(0f);
        ApplyColor(1f);
        SetVisible(false);
    }

    void OnEnable()
    {
        // PotionManager 등록
        TrySubscribe();
        UpdateTransform();
    }

    void OnDisable()
    {
        TryUnsubscribe();
    }

    void LateUpdate()
    {
        EnsureTarget();
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

    void EnsureTarget()
    {
        if (target == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc) target = pc.transform;
        }
    }

    void TrySubscribe()
    {
        if (PotionManager.instance == null) return;
        PotionManager.instance.OnGaugeStart += HandleGaugeStart;
        PotionManager.instance.OnGaugeProgress += HandleGaugeProgress;
        PotionManager.instance.OnGaugeEnd += HandleGaugeEnd;
    }

    void TryUnsubscribe()
    {
        if (PotionManager.instance == null) return;
        PotionManager.instance.OnGaugeStart -= HandleGaugeStart;
        PotionManager.instance.OnGaugeProgress -= HandleGaugeProgress;
        PotionManager.instance.OnGaugeEnd -= HandleGaugeEnd;
    }

    // ===== Potion Events =====
    void HandleGaugeStart(float duration)
    {
        _ratio = countDown ? 1f : 0f;
        ApplySize(_ratio);
        ApplyColor(_ratio);
        SetVisible(true);
    }

    void HandleGaugeProgress(float elapsed, float duration, float ratio)
    {
        _ratio = Mathf.Clamp01(countDown ? (1f - ratio) : ratio);
        ApplySize(_ratio);
        ApplyColor(_ratio);
    }

    void HandleGaugeEnd()
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

            // 왼쪽 기준으로 차오르도록 좌표 보정
            float leftEdge = -fullWidth * 0.5f;
            float fillCenterX = leftEdge + (w * 0.5f);
            fill.transform.localPosition = new Vector3(fillCenterX, 0f, 0f);
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