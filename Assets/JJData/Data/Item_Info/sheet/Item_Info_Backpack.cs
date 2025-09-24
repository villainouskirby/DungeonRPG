using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_Backpack
{
    /// <summary>
    /// 가방 id
    /// </summary>
    public string id;
    /// <summary>
    /// 최대 무게
    /// </summary>
    public float max_weight;
    /// <summary>
    /// 이동속도
    /// </summary>
    public float speed;
    /// <summary>
    /// 회피 강화
    /// </summary>
    public bool judge;
}
