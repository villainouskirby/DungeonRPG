using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_Item
{
    /// <summary>
    /// 아이템 id
    /// </summary>
    public string Item_id;
    /// <summary>
    /// 아이템 이름
    /// </summary>
    public string Item_name;
    /// <summary>
    /// 아이템 이미지
    /// </summary>
    public string Item_sprite;
    /// <summary>
    /// 아이템 최대개수
    /// </summary>
    public int Item_maxAmount;
    /// <summary>
    /// 랭크
    /// </summary>
    public int Item_rank;
    /// <summary>
    /// 무게
    /// </summary>
    public float Item_weight;
    /// <summary>
    /// 판매 가격
    /// </summary>
    public int Sell_price;
    /// <summary>
    /// 구매 가격
    /// </summary>
    public int Purchase_price;
    /// <summary>
    /// 아이템 사용가능 여부
    /// </summary>
    public bool Item_usable;
    /// <summary>
    /// 아이템 장착가능 여부
    /// </summary>
    public bool Item_wearable;
    /// <summary>
    /// 파우치 등록 여부
    /// </summary>
    public bool Item_pouchable;
    /// <summary>
    /// 아이템 상세정보 DT
    /// </summary>
    public string Item_PAR_DT;
}
