using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPool : MonoBehaviour
{
    [Header("Wall Prefab")]
    public GameObject WallPrefab;

    private Queue<PolygonCollider2D> _pool;

    public void Init(int viewChunkSize)
    {
        _pool = new(viewChunkSize * viewChunkSize);

        for (int i = 0; i < viewChunkSize * viewChunkSize; ++i)
        {
            Generate();
        }
    }

    public PolygonCollider2D Generate()
    {
        GameObject obj = Instantiate(WallPrefab, transform);
        obj.name = $"ChunkWall";
        obj.SetActive(false);

        PolygonCollider2D poly;
        poly = obj.GetComponent<PolygonCollider2D>();

        _pool.Enqueue(poly);

        return poly;
    }

    public PolygonCollider2D Get(PolygonColliderData data, Vector2Int chunkPos)
    {
        while (_pool.Count <= 0)
        {
            Generate();
        }

        PolygonCollider2D poly = _pool.Dequeue();
        poly.gameObject.SetActive(true);
        poly.gameObject.transform.position = new(chunkPos.x * 16, chunkPos.y * 16, 0);
        poly.pathCount = data.PathCount;
        for (int i = 0; i < data.Loops.Count; i++)
        {
            poly.SetPath(i, data.Loops[i]);
        }

        return poly;
    }

    public void Return(PolygonCollider2D poly)
    {
        poly.gameObject.SetActive(false);
        poly.pathCount = 0;

        _pool.Enqueue(poly);
    }
}
