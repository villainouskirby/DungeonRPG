using UnityEngine;

/// <summary>
/// Canvas 없이 SpriteRenderer만으로 체력바를 그리는 뷰.
/// Fill은 drawMode=Sliced(or Tiled) 상태에서 size.x를 줄여 길이를 바꾼다.
/// </summary>
[ExecuteAlways]
public class HPBarView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SpriteRenderer bg;
    [SerializeField] private SpriteRenderer fill;

    [Header("Visual")]
    [SerializeField] private float fullWidth = 1.5f; // 체력 100%일 때 가로폭(월드단위)
    [SerializeField] private float height = 0.2f; // 세로폭(월드단위)
    [SerializeField] private Color fullColor = new Color(0.25f, 0.95f, 0.25f);
    [SerializeField] private Color lowColor = new Color(0.95f, 0.25f, 0.25f);
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private float yOffset = 1.2f;   // 몬스터 머리 위 위치

    [Header("Sorting")]
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int orderOffset = 10;  // 몬스터 스프라이트보다 위

    private MonsterController owner;
    private float currentRatio = 1f;

    void Reset()
    {
        // 에디터에서 기본값 세팅
        if (bg) bg.drawMode = SpriteDrawMode.Sliced;
        if (fill) fill.drawMode = SpriteDrawMode.Sliced;
    }

    void Awake()
    {
        owner = GetComponentInParent<MonsterController>();
        SetupSorting();
        ApplySize(1f);
        ApplyColor(1f);
    }

    void OnEnable()
    {
        if (owner)
            owner.OnHpChanged += HandleHpChanged;
        if (owner)
            HandleHpChanged(owner.CurrentHP, owner.MaxHP);
        UpdateTransform();
    }

    void OnDisable()
    {
        if (owner)
            owner.OnHpChanged -= HandleHpChanged;
    }

    void LateUpdate()
    {
        // 항상 머리 위에 고정
        UpdateTransform();

#if UNITY_EDITOR
        // 에디터에서 값 변경 즉시 반영
        if (!Application.isPlaying)
        {
            SetupSorting();
            ApplySize(currentRatio);
            ApplyColor(currentRatio);
        }
#endif
    }

    void HandleHpChanged(float current, float max)
    {
        float r = (max <= 0f) ? 0f : current / max;
        r = Mathf.Clamp01(r);
        currentRatio = r;

        if (r <= 0f)
        {
            SetInVisible();
            return;
        }

        ApplySize(r);
        ApplyColor(r);

        if (hideWhenFull)
            SetVisible(r < 0.999f);
        else
            SetVisible(true);
    }

    void ApplySize(float ratio)
    {
        // SpriteRenderer.drawMode가 Sliced/Tiled일 때만 size 사용 가능
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

            // 왼쪽 기준으로 줄어들게: 가운데 기준이라 좌측 보정
            float leftEdge = -fullWidth * 0.5f;
            float fillCenterX = leftEdge + (w * 0.5f);
            fill.transform.localPosition = new Vector3(fillCenterX, 0f, 0f);
        }
    }

    void ApplyColor(float ratio)
    {
        if (!fill) return;

        // 초록→빨강 그라데이션
        Color c = Color.Lerp(lowColor, fullColor, ratio);
        // MaterialPropertyBlock 사용: 배치 드로우 최적화, 머티리얼 인스턴스화 방지
        var mpb = new MaterialPropertyBlock();
        fill.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", c);
        fill.SetPropertyBlock(mpb);
    }

    void SetupSorting()
    {
        int baseOrder = 0;
        var sr = owner ? owner.Sprite : null;
        if (sr) baseOrder = sr.sortingOrder;

        if (bg)
        {
            bg.sortingLayerName = string.IsNullOrEmpty(sortingLayer) ? (sr ? sr.sortingLayerName : "Default") : sortingLayer;
            bg.sortingOrder = baseOrder + orderOffset;
        }
        if (fill)
        {
            fill.sortingLayerName = string.IsNullOrEmpty(sortingLayer) ? (sr ? sr.sortingLayerName : "Default") : sortingLayer;
            fill.sortingOrder = baseOrder + orderOffset + 1;
        }
    }

    void UpdateTransform()
    {
        if (!owner) return;

        // 몬스터 머리 위로 오프셋
        var p = transform.parent ? transform.parent.position : Vector3.zero;
        transform.position = new Vector3(p.x, p.y + yOffset, p.z);
        transform.rotation = Quaternion.identity; // 2D라면 회전 고정(빌보드는 불필요)
    }

    void SetVisible(bool on)
    {
        if (bg) bg.enabled = on;
        if (fill) fill.enabled = on;
    }

    public void SetInVisible()
    {
        if (bg) bg.enabled = false;
        if (fill) fill.enabled = false;
    }
}