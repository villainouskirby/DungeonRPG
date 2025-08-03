using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_Potion
{
    /// <summary>
    /// 포션 id
    /// </summary>
    public string Potion_id;
    /// <summary>
    /// 효과
    /// </summary>
    public int Potion_effect;
    /// <summary>
    /// 회복
    /// </summary>
    public bool isHeal;
    /// <summary>
    /// 버프
    /// </summary>
    public bool isBuff;
    /// <summary>
    /// 설명
    /// </summary>
    public string Explanation;
}
