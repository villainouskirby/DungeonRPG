using UnityEngine;

public class Gold : MonoBehaviour
{
    [SerializeField] private GoldUI _goldUI;
    [SerializeField] private IntVariableSO _goldSO;
    [SerializeField] private int _maxGold;

    private void Awake()
    {
        _goldSO.OnValueChanged += ClampGold;
        ClampGold(_goldSO.Value);
    }

    private void ClampGold(int amount)
    {
        int clampedValue = Mathf.Clamp(amount, 0, _maxGold);
        _goldUI.ChangeValue(clampedValue);

        if (clampedValue != _goldSO.Value)
        {
            _goldSO.Value = clampedValue;
        }
    }

    public void AddGold()
    {
        _goldSO.Value++;
    }
}
