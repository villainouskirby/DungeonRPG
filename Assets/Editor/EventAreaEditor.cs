using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(EventArea))]
[CanEditMultipleObjects]
public class EventAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space(16);

        EventArea eventArea = (EventArea)target;

        string param1Msg = "Param1 : " + GetParam1Message(eventArea.Data.Type);
        string param2Msg = "Param2 : " + GetParam2Message(eventArea.Data.Type);
        string param3Msg = "Param3 : " + GetParam3Message(eventArea.Data.Type);
        EditorGUILayout.HelpBox(param1Msg, MessageType.None);
        EditorGUILayout.HelpBox(param2Msg, MessageType.None);
        EditorGUILayout.HelpBox(param3Msg, MessageType.None);
    }

    private string GetParam1Message(EventAreaType type)
    {
        return type switch
        {
            EventAreaType.ChangeLayer => "어떤 Layer로 변경할 것 인가",
            EventAreaType.ShowText => "띄울 text 내용",
            EventAreaType.OnOffLayer => "On할 Layer Indexs (/ 로 구분 ex) 1/2 - 1번 2번 레이어 On )",
            EventAreaType.OnOffSmoothLayer => "스무스하게 관리할 Layer Indexs (/ 로 구분 ex) 1/2 - 1번 2번 레이어 On )",
            EventAreaType.ChangeGroundLayer => "땅 레이어를 어떤 Layer로 변경할 것 인가",
            EventAreaType.ChangePlayerLight => "빛의 범위 - 기본이 10이니까 알아서 참고해서 변경",
            _ => "",
        };
    }

    private string GetParam2Message(EventAreaType type)
    {
        return type switch
        {
            EventAreaType.ChangeLayer => "없음",
            EventAreaType.ShowText => "없음",
            EventAreaType.OnOffLayer => "Off할 Layer Indexs (/ 로 구분 ex) 1/2 - 1번 2번 레이어 Off )",
            EventAreaType.OnOffSmoothLayer => "진행 방향 Up / Down / Right / Left",
            EventAreaType.ChangeGroundLayer => "땅 레이어란 플레이의 높이에 영향을 주는 레이어라고 생각하자",
            EventAreaType.ChangePlayerLight => "빛의 쌔기 - 기본이 1.5이니까 알아서 참고해서 변경",
            _ => "",
        };
    }

    private string GetParam3Message(EventAreaType type)
    {
        return type switch
        {
            EventAreaType.ChangeLayer => "없음",
            EventAreaType.ShowText => "Param1 : 종류 Wasd Click Dodge Crouch Sprint",
            EventAreaType.OnOffLayer => "없음",
            EventAreaType.OnOffSmoothLayer => "없음",
            EventAreaType.ChangeGroundLayer => "param2 : Up이라면 위에서 부터 아래를 0~1이라고 하고 이는 해당 레이어의 투명도이다. 즉 Up은 진입 시점이 0이고 나오는 시점이 1이다!",
            EventAreaType.ChangePlayerLight => "없음",
            _ => "",
        };
    }
}
