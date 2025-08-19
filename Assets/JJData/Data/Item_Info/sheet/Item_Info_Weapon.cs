using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Item_Info_Weapon
{
    /// <summary>
    /// 채집도구 테이블 id
    /// </summary>
    public string id;
    /// <summary>
    /// 공격력
    /// </summary>
    public int atk;
    /// <summary>
    /// 내구도
    /// </summary>
    public float durability;
    /// <summary>
    /// 1타 배율
    /// </summary>
    public float ratio1;
    /// <summary>
    /// 2타 배율
    /// </summary>
    public float ratio2;
    /// <summary>
    /// 강공격 배율
    /// </summary>
    public float strong_Ratio;
    /// <summary>
    /// 최대 차징 시간
    /// </summary>
    public float max_charge;
    /// <summary>
    /// 강공격 이동속도 감소
    /// </summary>
    public float strong_speed;
    /// <summary>
    /// 가드 성능
    /// </summary>
    public float guard_ratio;
    /// <summary>
    /// 저스트 가드 성능
    /// </summary>
    public float just_guard;
    /// <summary>
    /// 수리 비용
    /// </summary>
    public int repair_cost;
    /// <summary>
    /// 수리 재료
    /// </summary>
    public string repair_ing;
    /// <summary>
    /// 수리 재료 개수
    /// </summary>
    public int repair_ing_count;
    /// <summary>
    /// 설명
    /// </summary>
    public string Explanation;
}
