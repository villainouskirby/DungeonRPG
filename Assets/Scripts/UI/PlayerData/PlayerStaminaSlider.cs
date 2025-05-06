using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStaminaSlider : MonoBehaviour
{
    [Header("플레이어 스테미나 슬라이더")]
    [SerializeField] private Slider StaminaSlider;
    [SerializeField] private TMP_Text StaminaText;
    [Header("Max Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [Header("Current Stamina (게임 중 변동)")]
    [SerializeField] private FloatVariableSO currentStamina;
    private void Start()
    {
        StaminaSlider.minValue = 0f;
        StaminaSlider.maxValue = 1f;
        float StaminaRatio = currentStamina.Value / maxStamina;
        StaminaSlider.value = StaminaRatio;
    }
    private void Awake()
    {
    }
    private void Update()
    {
        float StaminaRatio = currentStamina.Value / maxStamina;

        // 슬라이더의 값 글자로 표현
        StaminaSlider.value = StaminaRatio;
        //StaminaText.text = $"{(int)currentStamina.Value}/{(int)maxStamina}";
    }
}