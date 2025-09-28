using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new DialogueSO", menuName = "DialogueScript")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueLineStatement> Lines;
    public DialogueEndEvent[] EndEvent;
}

[Serializable]
public struct DialogueEndEvent
{
    public enum KeyName
    {
        None,       // 이벤트 없음
        Dialogue,   // 다음 대사 출력
        Get,        // 아이템 획득
        Lose,       // 아이템 잃음
        Quest,      // 퀘스트 부여
    }

    public KeyName Key;
    public string Value;
    public int Amount;
}