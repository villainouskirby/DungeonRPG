using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Quest_Info_Gathering
{
    /// <summary>
    /// 퀘스트 목표 id
    /// </summary>
    public string id;
    /// <summary>
    /// 대상 오브젝트 id
    /// </summary>
    public string object_id;
    /// <summary>
    /// 목표 개수
    /// </summary>
    public int count;
}
