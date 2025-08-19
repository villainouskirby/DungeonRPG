using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Monster_Info_Monster
{
    /// <summary>
    /// 몬스터 id
    /// </summary>
    public string Monster_id;
    /// <summary>
    /// 몬스터 이름
    /// </summary>
    public string Monster_name;
    /// <summary>
    /// 랭크
    /// </summary>
    public int Monster_rank;
    /// <summary>
    /// 공격력
    /// </summary>
    public int Monster_atk;
    /// <summary>
    /// 체력
    /// </summary>
    public int Monster_hp;
    /// <summary>
    /// 이동속도
    /// </summary>
    public float Monster_speed;
    /// <summary>
    /// 탐지 레벨
    /// </summary>
    public int Monster_detection_level;
    /// <summary>
    /// 시야 발각 범위
    /// </summary>
    public int Monster_view_detection;
    /// <summary>
    /// 청각 발각 범위
    /// </summary>
    public int Monster_sound_detection;
    /// <summary>
    /// 특성 테이블
    /// </summary>
    public string Monster_property;
    /// <summary>
    /// 드롭 테이블
    /// </summary>
    public string Monster_DT;
    /// <summary>
    /// 특수 조건
    /// </summary>
    public string Monster_condition;
    /// <summary>
    /// 특수 드롭 테이블
    /// </summary>
    public string Monster_condition_DT;
}
