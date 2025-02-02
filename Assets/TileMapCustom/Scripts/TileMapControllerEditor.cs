using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMapController))]
public class TileMapControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TileMapController controller = (TileMapController)target;

        // 1. 기본 인스펙터 UI를 직접 그리기
        SerializedProperty iterator = serializedObject.GetIterator();
        iterator.NextVisible(true); // 첫 번째 프로퍼티로 이동

        while (iterator.NextVisible(false)) // 프로퍼티 순회
        {
            EditorGUILayout.PropertyField(iterator, true);

            // 특정 프로퍼티 후에 버튼 추가 (예제: tileTextures 뒤에 추가)
            if (iterator.name == "TileTexture")
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Create Texture2DArray"))
                {
                    controller.CreateTexture2DArray();
                }
                GUILayout.Space(10);
            }
            if (iterator.name == "TileSizeY")
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Create MapData and Buffer"))
                {
                    controller.InitializeTileMap();
                }
                GUILayout.Space(10);
            }
        }

        // 변경사항 적용
        serializedObject.ApplyModifiedProperties();
    }
}
