using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GaugeBarHandler : MonoBehaviour
{
    [Tooltip("비율로 표시할건지의 여부 => false 시 currentValue만 출력")]
    [SerializeField] private bool _isShowRate = true;

    [Header("Variables")]
    [SerializeField] private FloatVariableSO _currentValue;
    [SerializeField] private FloatVariableSO _maxValue;

    [Header("Gauge")]
    [SerializeField] private Image _guageImage;
    [SerializeField] private TextMeshProUGUI _guageText;

    private float MaxValue => _maxValue != null ? _maxValue.Value : 100;

    private void OnEnable()
    {
        if (_currentValue == null) return;

        _currentValue.OnValueChanged += UpdateGauge;
        UpdateGauge(_currentValue.Value);
    }

    private void OnDisable()
    {
        if (_currentValue == null) return;

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
            string txt;

            if (_isShowRate)
            {
                txt = $"{(int)value}/{(int)MaxValue}";
            }
            else
            {
                txt = ((int)value).ToString();
            }

            _guageText.text = txt;
        }
    }
}
