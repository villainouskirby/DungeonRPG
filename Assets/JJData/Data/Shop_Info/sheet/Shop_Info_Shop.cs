using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Shop_Info_Shop
{
    /// <summary>
    /// 아이템 id
    /// </summary>
    public string item_id;
    /// <summary>
    /// 판매 개수
    /// </summary>
    public int purchase_count;
    /// <summary>
    /// 판매 가격
    /// </summary>
    public int purchase_price;
    /// <summary>
    /// 판매 시점
    /// </summary>
    public int unlock;
}
