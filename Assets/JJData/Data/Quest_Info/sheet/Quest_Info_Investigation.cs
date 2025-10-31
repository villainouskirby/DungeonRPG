using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Quest_Info_Investigation
{
    /// <summary>
    /// 퀘스트 목표 id
    /// </summary>
    public string id;
    /// <summary>
    /// 퀘스트 목표
    /// </summary>
    public string Goal;
    /// <summary>
    /// 조사 종류
    /// </summary>
    public string type;
    /// <summary>
    /// 대상 오브젝트
    /// </summary>
    public string object_id;
}
