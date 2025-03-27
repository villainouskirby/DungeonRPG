using TMPro;
using UnityEngine;

public class EquipmentEffectUI : MonoBehaviour
{
    [SerializeField] private EquipmentEffectSO _equipmentEffectSO;

    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _staminaText;
    [SerializeField] private TextMeshProUGUI _additionalEffectText;

    public void SetEffectTexts()
    {
        _damageText.text = "Damage " + _equipmentEffectSO.Damage.ToString();
        _hpText.text = "Hp" + _equipmentEffectSO.Hp.ToString();
        _staminaText.text = "Stamina" + _equipmentEffectSO.Stamina.ToString();
        //_additionalEffectText.text = _equipmentEffectSO.AdditionalEffects.ToString(); // TODO => 저장 방식에 따른 나열방식로직 짜기
    }

    private void Awake()
    {
        // 데이터 초기화
        _equipmentEffectSO.Damage = 0;
        _equipmentEffectSO.Hp = 0;
        _equipmentEffectSO.Stamina = 0;
        SetEffectTexts();
    }
}
