using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(ResourceNodeSpawnerTile))]
[CanEditMultipleObjects]
public class SpawnerTileCustomEditor : Editor
{
    public string[] Options => ResourceNodeListManager.ResourceNodeList;

    private ReorderableList _list;

    private void OnEnable()
    {
        var prop = serializedObject.FindProperty("ResourceNodes");
        _list = new(serializedObject, prop, true, true, true, true);
        _list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "ResourceNode Selections");
        };

        _list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = prop.GetArrayElementAtIndex(index);
            
            int currentIndex = System.Array.IndexOf(Options, element.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            element.stringValue = Options[
                EditorGUI.Popup(
                    new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    currentIndex,
                    Options
                )
            ];
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        EditorGUILayout.Space(16);
        _list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
