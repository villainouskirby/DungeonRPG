using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_Item
{
    /// <summary>
    /// 아이템 id
    /// </summary>
    public string id;
    /// <summary>
    /// 아이템 타입
    /// </summary>
    public string type;
    /// <summary>
    /// 아이템 이름
    /// </summary>
    public string name;
    /// <summary>
    /// 아이템 이미지
    /// </summary>
    public string sprite;
    /// <summary>
    /// 아이템 최대개수
    /// </summary>
    public int max_amount;
    /// <summary>
    /// 랭크
    /// </summary>
    public int rank;
    /// <summary>
    /// 무게
    /// </summary>
    public float weight;
    /// <summary>
    /// 판매 가격
    /// </summary>
    public int sell_price;
    /// <summary>
    /// 아이템 투척가능 여부
    /// </summary>
    public bool throwable;
    /// <summary>
    /// 아이템 사용가능 여부
    /// </summary>
    public bool usable;
    /// <summary>
    /// 파우치 등록 여부
    /// </summary>
    public bool pouchable;
    /// <summary>
    /// 판매 가능 여부
    /// </summary>
    public bool sellable;
    /// <summary>
    /// 설명
    /// </summary>
    public string Explanation;
}
