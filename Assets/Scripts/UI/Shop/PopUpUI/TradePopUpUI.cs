using TMPro;
using Unity.VisualScripting;
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
        Item item = _shopUI.GetItemData(index);

        switch (_shopUI.Type)
        {
            case ShopType.purchase:
                _popUpName.text = "Buy";
                _price = item.Data.Info.Purchase_price;
                int maxAmount = (_price <= 0) ? 99 : _shopUI.GetCurrentGold() / _price;

                SetItemData(index, item.Data.IconSprite, item.Data.Name, maxAmount);
                break;

            case ShopType.sell:
                _popUpName.text = "Sell";
                _price = item.Data.Info.Sell_price;
                int amount;

                if (item is CountableItem ci)
                {
                    amount = ci.Amount;
                }
                else
                {
                    amount = 1;
                }

                SetItemData(index, item.Data.IconSprite, item.Data.Name, amount);
                break;

            default:
                Debug.Log("상점 타입 에러");
                return;
        }

        UpdateGoldText("1");
    }

    protected override void ExecuteConfirm()
    {
        _shopUI.Trade(_selectedIndex, _inputAmount);
    }
}
