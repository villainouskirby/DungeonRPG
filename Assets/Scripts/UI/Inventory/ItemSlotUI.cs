using TMPro;
using UnityEngine;

public class ItemSlotUI : SlotUI
{
    [Tooltip("아이템 이름")]
    [SerializeField] private TextMeshProUGUI _nameText;

    [Tooltip("아이템 개수")]
    [SerializeField] private TextMeshProUGUI _amountText;

    [Tooltip("아이템 개당 무게")]
    [SerializeField] private TextMeshProUGUI _weightText;

    [Tooltip("아이템 총 무게")]
    [SerializeField] private TextMeshProUGUI _weightSumText;

    private float _itemWeight;
    
    public void SetItemInfo(Sprite imageSprite, string name, float Weight)
    {
        SetItemInfo(imageSprite);
        _nameText.text = name;
        _itemWeight = Weight;
        _weightText.text = Weight.ToString() + "kg";
        gameObject.SetActive(true);
    }

    public void SetItemAmount(int amount)
    {
        if (amount == -1)
        {
            _amountText.text = "";
            _weightSumText.text = "";
        }
        else
        {
            _amountText.text = "X " + amount.ToString();
            _weightSumText.text = (amount * _itemWeight).ToString() + "kg";
        }
    }
}
