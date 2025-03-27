using System;

[Serializable]
/// <summary> 상점 품목 </summary>
public class ShopItem : Item
{
    /// <summary> 가격 </summary>
    public int Price { get; private set; }

    /// <summary> 수량 </summary>
    public int Amount { get; private set; }
    private int _maxAmount = 99;

    public ShopItem(ItemData itemData, int price, int amount = -1) : base(itemData)
    {
        Price = price;
        SetAmount(amount);
    }

    /// <summary> 개수 지정(범위 제한)</summary>
    public void SetAmount(int amount)
    {
        Amount = Math.Clamp(amount, 0, _maxAmount);
    }

    /// <summary> 개수 추가 </summary>
    /// <returns> 최대치 초과량(없으면 0) </returns>
    public int AddAmountAndGetExcess(int amount)
    {
        int nextAmount = Amount + amount;
        SetAmount(nextAmount);

        return (nextAmount > _maxAmount) ? (nextAmount - _maxAmount) : 0;
    }

    public override Item Clone()
    {
        return new ShopItem(Data, Price);
    }
}
