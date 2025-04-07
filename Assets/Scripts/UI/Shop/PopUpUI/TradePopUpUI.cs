using TMPro;
using UnityEngine;

public class TradePopUpUI : AmountSelectPopUpUI
{
    [SerializeField] private TextMeshProUGUI _goldText;

    [Header("Shop UI")]
    [SerializeField] protected ShopUI _shopUI;

    protected int _price;

    protected override void Awake()
    {
        base.Awake();
        _amountInput.onValueChanged.AddListener(UpdateGoldText);
    }

    private void UpdateGoldText(string amount)
    {
        int value = int.Parse(amount) * _price;
        _goldText.text = value.ToString();
        if (value > _shopUI.GetCurrentGold()) _goldText.color = Color.red;
        else _goldText.color = Color.black;
    }

    public override void SetItemData(int index)
    {
        ShopItem shopItem = _shopUI.GetItemData(index);
        ItemData itemData = shopItem.Data;
        _price = shopItem.Price;
        if (_shopUI.Type == ShopType.buy)
        {
            _popUpName.text = "Buy";
            SetItemData(index, itemData.IconSprite, itemData.Name, _shopUI.GetCurrentGold() / _price);
        }
        else
        {
            _popUpName.text = "Sell";
            SetItemData(index, itemData.IconSprite, itemData.Name, shopItem.Amount);
        }

        UpdateGoldText("1");
    }

    protected override void ExecuteConfirm()
    {
        _shopUI.Trade(_selectedIndex, _inputAmount);
    }
}
