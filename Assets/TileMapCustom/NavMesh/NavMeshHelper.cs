using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class NavMeshHelper : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // 현재 활성화된 NavMeshData의 삼각분할 결과를 가져온다
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        Vector3[] verts = tri.vertices;
        int[] idx = tri.indices;

        if (verts.Length == 0 || idx.Length == 0)
        {
            // 빈 NavMesh
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.one * 8f, new Vector3(16f, 16f, 1f));
            return;
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < idx.Length; i += 3)
        {
            Vector3 a = verts[idx[i + 0]];
            Vector3 b = verts[idx[i + 1]];
            Vector3 c = verts[idx[i + 2]];
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}
