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
            _ => "",
        };
    }

    private string GetParam3Message(EventAreaType type)
    {
        return type switch
        {
            EventAreaType.ChangeLayer => "없음",
            EventAreaType.ShowText => "없음",
            EventAreaType.OnOffLayer => "없음",
            _ => "",
        };
    }
}
