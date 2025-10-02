using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GaugeBarHandler : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private FloatVariableSO _currentValue;
    [SerializeField] private FloatVariableSO _maxValue;

    [Header("Gauge")]
    [SerializeField] private Image _guageImage;
    [SerializeField] private TextMeshProUGUI _guageText;

    private float MaxValue => _maxValue.Value;

    private void OnEnable()
    {
        _currentValue.OnValueChanged += UpdateGauge;
        UpdateGauge(_currentValue.Value);
    }

    private void OnDisable()
    {
        _currentValue.OnValueChanged -= UpdateGauge;
    }

    public void UpdateGauge(float value)
    {
        if (_guageImage != null)
        {
            _guageImage.fillAmount = value / MaxValue;
        }

        if (_guageText != null)
        {
            _guageText.text = $"{(int)value}/{(int)MaxValue}";
        }
    }
}
