using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class AmountSelectPopUpUI : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _popUpName;

    [Header("Options")]
    [SerializeField] protected TMP_InputField _amountInput;
    [SerializeField] protected Button _addButton;
    [SerializeField] protected Button _subButton;
    [SerializeField] protected Button _maxButton;
    [SerializeField] protected Button _minButton;
    [SerializeField] protected Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    [Header("ItemData")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;

    protected int _selectedIndex;
    protected int _inputAmount;
    protected int _maxInputAmount;
    private readonly int _minInputAmount = 1;

    public abstract void SetItemData(int index);

    protected void SetItemData(int index, Sprite sprite, string name, int maxInputAmount)
    {
        _selectedIndex = index;
        _icon.sprite = sprite;
        _itemName.text = name;
        _maxInputAmount = maxInputAmount;
        _amountInput.text = "1";

        gameObject.SetActive(true);
    }

    protected virtual void Awake()
    {
        _amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        _addButton.onClick.AddListener(() => AddAmountText(1));
        _subButton.onClick.AddListener(() => AddAmountText(-1));
        _maxButton.onClick.AddListener(() => SetAmountText(_maxInputAmount));
        _minButton.onClick.AddListener(() => SetAmountText(_minInputAmount));
        _confirmButton.onClick.AddListener(Confirm);
        _cancelButton.onClick.AddListener(Close);
    }

    private void SetAmountText(int amount)
    {
        _amountInput.text = Mathf.Clamp(amount, 1, _maxInputAmount).ToString();
    }

    private void AddAmountText(int amount)
    {
        SetAmountText(int.Parse(_amountInput.text) + amount);
    }

    /// <summary> 문제가 없을시 실행될 함수 </summary>
    protected abstract void ExecuteConfirm();

    private void Confirm()
    {
        _inputAmount = int.Parse(_amountInput.text);

        // 입력값이 정상 범위내에 있는지 확인
        if (_inputAmount < 1 || _inputAmount > _maxInputAmount)
        {
            SetAmountText(_inputAmount); // 데이터 정상범위내로 설정
        }
        else
        {
            ExecuteConfirm();
            Close();
        }
    }

    public void Close()
    {
        ResetData();
        gameObject.SetActive(false);
    }

    private void ResetData()
    {
        _icon.sprite = null;
        _itemName.text = "";
        _selectedIndex = -1;
        _maxInputAmount = -1;
    }
}
