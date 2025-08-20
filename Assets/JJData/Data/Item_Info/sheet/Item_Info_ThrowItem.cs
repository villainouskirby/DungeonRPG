using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_ThrowItem
{
    /// <summary>
    /// 아이템 id
    /// </summary>
    public string id;
    /// <summary>
    /// 파우치 최대 등록 개수
    /// </summary>
    public int max_register_count;
    /// <summary>
    /// 데미지
    /// </summary>
    public float damage;
    /// <summary>
    /// 사용 거리
    /// </summary>
    public int use_distance;
    /// <summary>
    /// 소리 범위
    /// </summary>
    public float sound_range;
    /// <summary>
    /// 설명
    /// </summary>
    public string Explanation;
}
