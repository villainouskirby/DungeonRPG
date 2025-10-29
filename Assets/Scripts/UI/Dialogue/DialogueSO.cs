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
        None,               // 이벤트 없음
        Dialogue,           // 다음 대사 출력
        Get,                // 아이템 획득
        Lose,               // 아이템 잃음
        AcceptQuest,        // 퀘스트 수주
        UnlockQuest,        // 퀘스트 해금
        Palette,            // 팔레트 팝업 => value(bag, quest, map, book), amount(0 : 기본, 1 : 점, 2 : 빛)
        Tutorial,           // 튜토리얼 팝업 => value(Bag, Quest, Map, Click 등등 자세한건 KeyGuideUI 스크립트에서 확인), amount(0 : 꺼짐, 1 : 켜짐)
        CloseDialogue,      // 대화 종료
        FocusUI,            // UI 포커싱 => value(FocusTarget의 ID 혹은 아이템 ID), Amount(0 : FocustTarget, 1 : Inventory 아이템 슬롯, 2 : FocusTarget(일반 클릭 이벤트))
    }

    public KeyName Key;
    public string Value;
    public int Amount;

    [Header("FocusUI")]
    [Multiline] public string TutorialText;
    public Vector2 TextPosition;
}