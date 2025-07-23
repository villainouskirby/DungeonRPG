using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드-스페이스 차지 게이지를 한 곳에서 관리
/// </summary>
public class ChargeUIController : MonoBehaviour
{
    [Header("World-Space UI")]
    [SerializeField] Canvas chargeCanvas;          // RenderMode = World Space
    [SerializeField] CanvasGroup canvasGroup;      // 알파 0 ↔ 1
    [SerializeField] Slider attackSlider;
    [SerializeField] Slider potionSlider;
    [SerializeField] Vector3 offset = new(0f, 1.2f, 0f);

    public void ShowAttackGauge() => Activate(attackSlider);
    public void ShowPotionGauge() => Activate(potionSlider);
    public void HideAll() => canvasGroup.alpha = 0f;

    public void SetAttackRatio(float r) => attackSlider.value = r;
    public void SetPotionRatio(float r) => potionSlider.value = r;

    void FixedUpdate()            // 플레이어 따라다니기 + 카메라 정면 유지
    {
        if (!chargeCanvas) return;
        chargeCanvas.transform.position = transform.position + offset;
        chargeCanvas.transform.forward = Camera.main.transform.forward;
    }

    /* ---------- 내부 ---------- */
    void Activate(Slider target)
    {
        attackSlider.gameObject.SetActive(target == attackSlider);
        potionSlider.gameObject.SetActive(target == potionSlider);

        attackSlider.value = 0f;
        potionSlider.value = 0f;

        canvasGroup.alpha = 1f;
    }
}