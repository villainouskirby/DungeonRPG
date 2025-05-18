using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHelper : MonoBehaviour
{
    public int ChunkViewSize = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        // 이 오브젝트 위치에 와이어프레임 구 그리기

        for (int i = 0; i < ChunkViewSize; i++)
        {
            for (int j = 0; j < ChunkViewSize; j++)
            {
                Gizmos.color = GetColor(i, j);
                Gizmos.DrawWireCube(new Vector3(i * 16 + 8,j * 16 + 8), new Vector3(16, 16, 0));
            }
        }
    }

    private Color[] gizmoColorArray = new Color[] { Color.cyan, Color.red, Color.blue, Color.green };
    private Color GetColor(int i, int j)
    {
        int a = i + j;
        a = a % gizmoColorArray.Length;
        return gizmoColorArray[a];
    }
}
