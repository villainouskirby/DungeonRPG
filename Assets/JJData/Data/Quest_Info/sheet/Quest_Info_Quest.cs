using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public  class Quest_Info_Quest
{
    /// <summary>
    /// 퀘스트 id
    /// </summary>
    public string id;
    /// <summary>
    /// 퀘스트 수주 NPC
    /// </summary>
    public string npc;
    /// <summary>
    /// 퀘스트 이름
    /// </summary>
    public string name;
    /// <summary>
    /// 퀘스트 수주 텍스트
    /// </summary>
    public string start_text;
    /// <summary>
    /// 퀘스트 조건 id1
    /// </summary>
    public string con_id1;
    /// <summary>
    /// 퀘스트 조건 id2
    /// </summary>
    public string con_id2;
    /// <summary>
    /// 퀘스트 조건 id3
    /// </summary>
    public string con_id3;
    /// <summary>
    /// 퀘스트 보상 정보
    /// </summary>
    public string reward_info;
    /// <summary>
    /// 퀘스트 완료 텍스트
    /// </summary>
    public string end_text;
    /// <summary>
    /// 해금되는 퀘스트 id
    /// </summary>
    public string unlock_id;
    /// <summary>
    /// 퀘스트 내용
    /// </summary>
    public string explaination;
}
