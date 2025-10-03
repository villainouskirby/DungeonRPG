#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InGameChynkHelper : MonoBehaviour
{
    GUIStyle _style;

    private void Awake()
    {
        _style = new GUIStyle
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (ChunkManager.Instance.LoadedChunkIndex == null)
            return;

        foreach(var chunkIndex in ChunkManager.Instance.LoadedChunkIndex)
        {
            bool flag = false;
            for (int i = -ChunkManager.Instance.ChunkBlank; i <= ChunkManager.Instance.ChunkBlank; i++)
            {
                for (int j = -ChunkManager.Instance.ChunkBlank; j <= ChunkManager.Instance.ChunkBlank; j++)
                {
                    if (ChunkManager.Instance.LastChunkPos + new Vector2Int(i, j) == chunkIndex.Key)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireCube(new(chunkIndex.Key.x * 16 + 8, chunkIndex.Key.y * 16 + 8, 0), new(15.8f, 15.8f, 0));
                        Handles.Label(new(chunkIndex.Key.x * 16 + 8, chunkIndex.Key.y * 16 + 8, 0), $"{chunkIndex.Value} Chunk", _style);

                        flag = true;
                    }
                }
            }
            if (flag)
                continue;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new(chunkIndex.Key.x * 16 + 8, chunkIndex.Key.y * 16 + 8, 0), new(15.8f, 15.8f, 0));
            Handles.Label(new(chunkIndex.Key.x * 16 + 8, chunkIndex.Key.y * 16 + 8, 0), $"{chunkIndex.Value} Chunk", _style);
        }
    }
}
#endif