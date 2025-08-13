using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_Pouch
{
    /// <summary>
    /// 파우치 id
    /// </summary>
    public string id;
    /// <summary>
    /// 파우치 개수
    /// </summary>
    public int count;
    /// <summary>
    /// 회복량 증가
    /// </summary>
    public float heal_buff;
    /// <summary>
    /// 설명
    /// </summary>
    public string Explanation;
}
