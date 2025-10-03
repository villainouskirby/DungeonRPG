using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using CM = ChunkManager;
using DL = DataLoader;

public class ShadowManager : MonoBehaviour, ITileMapBase
{
    public Dictionary<Vector2Int, ChunkShadowCaster2D> ActiveShadow;
    private Dictionary<Vector2Int, AsyncOperationHandle> _handleDic;
    private MapEnum _currentMapType;

    public GameObject ShadowRoot;
    public ShadowPool Pool;

    public void Init()
    {
        _handleDic = new();
        ActiveShadow = new();

        Pool.Init(CM.Instance.ViewChunkSize);
    }

    public void InitMap(MapEnum mapType)
    {
        List<Vector2Int> target = new();
        foreach(var pair in ActiveShadow)
            target.Add(pair.Key);
        for (int i = 0; i < target.Count; i++)
            UnLoadShadow(target[i]);

        CM.Instance.ChunkLoadAction += LoadShadow;
        CM.Instance.ChunkUnloadAction += UnLoadShadow;
        CM.Instance.ChunkRefreshAction += UnLoadShadow;
        CM.Instance.ChunkRefreshAction += LoadShadow;

        _currentMapType = mapType;
    }

    public void StartMap(MapEnum mapType)
    {
        InitShadow();
    }

    public void InitShadow()
    {
        foreach (var key in CM.Instance.LoadedChunkIndex.Keys)
        {
            LoadShadow(key);
        }
    }

    private void LoadShadow(Vector2Int chunkPos)
    {
        if (!(chunkPos.x >= 0 && chunkPos.x < DL.Instance.All.Width && chunkPos.y >= 0 && chunkPos.y < DL.Instance.All.Height))
            return;

        if (!_handleDic.ContainsKey(chunkPos))
            return;
        _handleDic[chunkPos] = Addressables.LoadAssetAsync<Mesh>($"{_currentMapType.ToString()}_layer{HeightManager.Instance.CurrentLayer}_ChunkShadowMesh_{chunkPos.x}_{chunkPos.y}");

        _handleDic[chunkPos].Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                SetShadow((Mesh)op.Result, chunkPos);
        };
    }

    private void SetShadow(Mesh mesh, Vector2Int chunkPos)
    {
        ActiveShadow[chunkPos] = Pool.Get(mesh, chunkPos);
    }

    private void UnLoadShadow(Vector2Int chunkPos)
    {
        if (!(chunkPos.x >= 0 && chunkPos.x < DL.Instance.All.Width && chunkPos.y >= 0 && chunkPos.y < DL.Instance.All.Height))
            return;

        if (ActiveShadow.ContainsKey(chunkPos))
        {
            Pool.Return(ActiveShadow[chunkPos]);
            ActiveShadow.Remove(chunkPos);
            Addressables.Release(_handleDic[chunkPos]);
            _handleDic.Remove(chunkPos);
        }
    }

    public int Prime => (int)TileMapBasePrimeEnum.ShadowManager;
}
