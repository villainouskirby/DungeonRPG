using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;      // 인스펙터에서 Slider 드래그
    private Transform target;                    // 따라갈 몬스터
    private Vector3 offset;                      // 머리 위 오프셋

    // 체력바가 추적할 대상과 오프셋을 설정
    public void Init(Transform target, Vector3 offset)
    {
        this.target = target;
        this.offset = offset;

        // 안전 장치: Slider 설정 (0~1 범위)
        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }
    }

    private void LateUpdate()
    {
        if (!target) { Destroy(gameObject); return; }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        transform.position = screenPos;
    }

    public void SetRatio(float ratio)
    {
        if (slider) slider.value = Mathf.Clamp01(ratio);
    }
}