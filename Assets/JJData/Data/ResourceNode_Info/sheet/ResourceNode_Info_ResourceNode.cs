using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class ResourceNode_Info_ResourceNode
{
    /// <summary>
    /// 채집물 ID
    /// </summary>
    public string ResourceNode_id;
    /// <summary>
    /// 채집물 이름
    /// </summary>
    public string ResourceNode_name;
    /// <summary>
    /// 채집물 이미지
    /// </summary>
    public string ResourceNode_sprite;
    /// <summary>
    /// 채집 저항력
    /// </summary>
    public int Resistance;
    /// <summary>
    /// 체력
    /// </summary>
    public int Hp;
    /// <summary>
    /// 채집 가능 횟수
    /// </summary>
    public int Gathering_count;
    /// <summary>
    /// 파괴 가능
    /// </summary>
    public bool isDestructible;
    /// <summary>
    /// 깨지기 쉬움
    /// </summary>
    public bool isFragile;
    /// <summary>
    /// 상호작용 DT
    /// </summary>
    public string DT_interaction;
    /// <summary>
    /// 파괴 DT
    /// </summary>
    public string DT_destroy;
    /// <summary>
    /// 상호작용 마지막 DT
    /// </summary>
    public string DT_lastInteraction;
    /// <summary>
    /// 특수 조건
    /// </summary>
    public string Condition;
    /// <summary>
    /// 특수조건 DT
    /// </summary>
    public string DT_condition;
    /// <summary>
    /// 설명
    /// </summary>
    public string Explaination;
}
