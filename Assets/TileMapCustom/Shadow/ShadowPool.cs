using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPool : MonoBehaviour
{
    [Header("Chunkhadow Prefab")]
    public GameObject ChunkShadowPrefab;

    private Queue<ChunkShadowCaster2D> _pool;

    public void Init(int viewChunkSize)
    {
        _pool = new(viewChunkSize * viewChunkSize);

        for(int i = 0; i < viewChunkSize * viewChunkSize; ++i)
        {
            Generate();
        }
    }

    public ChunkShadowCaster2D Generate()
    {
        GameObject obj = Instantiate(ChunkShadowPrefab, transform);
        obj.name = $"ChunkShadow";
        obj.SetActive(false);

        ChunkShadowCaster2D ChunkShadowCaster;
        ChunkShadowCaster = obj.GetComponent<ChunkShadowCaster2D>();

        _pool.Enqueue(ChunkShadowCaster);

        return ChunkShadowCaster;
    }

    public ChunkShadowCaster2D Get(Mesh mesh, Vector2Int chunkPos)
    {
        while (_pool.Count <= 0)
        {
            Generate();
        }

        ChunkShadowCaster2D obj = _pool.Dequeue();
        obj.transform.position = new(chunkPos.x * 16, chunkPos.y * 16, 0);
        obj.ApplyChunkShadow(mesh);
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void Return(ChunkShadowCaster2D obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
