using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemovePopUpUI : MonoBehaviour
{
    [SerializeField] private InventoryUI _inventoryUI;

    [Header("ItemData")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;

    [Header("RemovePopUp Option")]
    [SerializeField] private TMP_InputField _amountInput;
    [SerializeField] private Button _addButton;
    [SerializeField] private Button _subButton;
    [SerializeField] private Button _maxButton;
    [SerializeField] private Button _minButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private int _selectedIndex;
    private int _maxInputAmount;
    private readonly int _minInputAmount = 1;

    public void SetItemData(int index)
    {
        ItemData itemData = _inventoryUI.GetItemData(index);

        _icon.sprite = itemData.IconSprite;
        _itemName.text = itemData.Name;
        _amountInput.text = "1";

        _selectedIndex = index;

        _maxInputAmount = _inventoryUI.GetItemAmount(index);

        gameObject.SetActive(true);
    }

    private void Awake()
    {
        _amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        _addButton.onClick.AddListener(() => AddAmountText(1));
        _subButton.onClick.AddListener(() => AddAmountText(-1));
        _maxButton.onClick.AddListener(() => SetAmountText(_maxInputAmount));
        _minButton.onClick.AddListener(() => SetAmountText(_minInputAmount));
        _confirmButton.onClick.AddListener(ConfirmRemove);
        _cancelButton.onClick.AddListener(EndRemove);
    }

    private void SetAmountText(int amount)
    {
        _amountInput.text = Mathf.Clamp(amount, 1, _maxInputAmount).ToString();
    }

    private void AddAmountText(int amount)
    {
        SetAmountText(int.Parse(_amountInput.text) + amount);
    }
    private void ConfirmRemove()
    {
        int amount = int.Parse(_amountInput.text);

        // 입력값이 정상 범위내에 있는지 확인
        if (amount < 1 || amount > _maxInputAmount)
        {
            SetAmountText(amount); // 데이터 정상범위내로 설정
        }
        else
        {
            if (amount == _maxInputAmount) _inventoryUI.RemoveItem(_selectedIndex);
            else _inventoryUI.RemoveItem(_selectedIndex, amount);

            EndRemove();
        }
    }

    private void EndRemove()
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
