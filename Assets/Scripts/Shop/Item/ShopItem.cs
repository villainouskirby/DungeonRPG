using Unity.VisualScripting;
using UnityEngine; // 디버그용
/// <summary> 상점 품목 </summary>
public class ShopItem
{
    /// <summary> 판매 / 구매 아이템 </summary>
    public Item Item { get; private set; }
    /// <summary> 가격 </summary>
    public int Price { get; private set; }
    /// <summary> 재고 수량 </summary>
    public int Stock { get; private set; }
    /// <summary> 구매 가능 여부 </summary>
    public bool IsAvailable => Stock > 0;

    public ShopItem(Item item, int price, int stock = 0)
    {
        Item = item;
        Price = price;
        Stock = (item is CountableItem) ? stock : 1;
    }

    /// <summary> 거래(판매/구매) </summary>
    /// <returns> 거래된 객체(복제됨) </returns>
    public Item Trade(int amount)
    {
        Stock -= amount;
        if (Item is CountableItem ci)
        {
            Debug.Log(ci.Clone(amount).Data.Name + " " + amount + "개 구입");
            return ci.Clone(amount);
        }
        else
        {
            return Item.Clone();
        }
    }
}
