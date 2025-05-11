using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHPSlider : MonoBehaviour
{
    [Header("플레이어 HP 슬라이더")]
    [SerializeField] private Slider HpSlider;
    [SerializeField] private TMP_Text HPText;
    [Header("Max HP")]
    [SerializeField] private FloatVariableSO maxHP;
    [Header("Current HP (게임 중 변동)")]
    [SerializeField] private FloatVariableSO currentHP;
    private void Start()
    {
        HpSlider.minValue = 0f;
        HpSlider.maxValue = 1f;
        float hpRatio = currentHP.Value / maxHP.Value;
        HpSlider.value = hpRatio;
    }
    private void Awake()
    {
    }
    private void Update()
    {
        float hpRatio = currentHP.Value / maxHP.Value;

        // 슬라이더의 값 글자로 표현
        HpSlider.value = hpRatio;
        HPText.text = $"{(int)currentHP.Value}/{(int)maxHP.Value}";
    }
}