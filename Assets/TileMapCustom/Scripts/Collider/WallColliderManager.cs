using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using CM = ChunkManager;
using DL = DataLoader;

public class WallColliderManager : MonoBehaviour, ITileMapBase
{
    public Dictionary<Vector2Int, PolygonCollider2D> ActiveWall;
    private Dictionary<Vector2Int, AsyncOperationHandle> _handleDic;
    private MapEnum _currentMapType;

    public WallPool Pool;

    public void Init()
    {
        _handleDic = new();
        ActiveWall = new();

        Pool.Init(CM.Instance.ViewChunkSize);
    }

    public void InitMap(MapEnum mapType)
    {
        List<Vector2Int> target = new();
        foreach (var pair in ActiveWall)
            target.Add(pair.Key);
        for (int i = 0; i < target.Count; i++)
            UnLoadWall(target[i]);

        CM.Instance.ChunkLoadAction += LoadWall;
        CM.Instance.ChunkUnloadAction += UnLoadWall;
        CM.Instance.ChunkRefreshAction += UnLoadWall;
        CM.Instance.ChunkRefreshAction += LoadWall;

        _currentMapType = mapType;
    }

    public void StartMap(MapEnum mapType)
    {
        InitWall();
    }

    public void RefreshLayer()
    {
        List<Vector2Int> target = new();
        foreach (var pair in ActiveWall)
            target.Add(pair.Key);
        for (int i = 0; i < target.Count; i++)
            UnLoadWall(target[i]);
        InitWall();
    }

    public void InitWall()
    {
        foreach (var key in CM.Instance.LoadedChunkIndex.Keys)
        {
            LoadWall(key);
        }
    }


    public bool HasKey(string key)
    {
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations))
            {
                return true; // 키 존재
            }
        }
        return false; // 키 없음
    }

    private void LoadWall(Vector2Int chunkPos)
    {
        if (!(chunkPos.x >= 0 && chunkPos.x < DL.Instance.All.Width && chunkPos.y >= 0 && chunkPos.y < DL.Instance.All.Height))
            return;

        if (ActiveWall.ContainsKey(chunkPos))
            return;

        if (!HasKey($"{_currentMapType.ToString()}_layer{HeightManager.Instance.CurrentLayer}_WallMesh_{chunkPos.x}_{chunkPos.y}"))
            return;

        _handleDic[chunkPos] = Addressables.LoadAssetAsync<TextAsset>($"{_currentMapType.ToString()}_layer{HeightManager.Instance.CurrentLayer}_WallMesh_{chunkPos.x}_{chunkPos.y}");

        _handleDic[chunkPos].Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                SetWall(TypeByte2TypeConverter.Convert<PolygonColliderData>(((TextAsset)op.Result).bytes), chunkPos);
        };
    }

    private void SetWall(PolygonColliderData data, Vector2Int chunkPos)
    {
        if (ActiveWall.ContainsKey(chunkPos))
            return;
        ActiveWall[chunkPos] = Pool.Get(data, chunkPos);
    }

    private void UnLoadWall(Vector2Int wallPos)
    {
        if (!(wallPos.x >= 0 && wallPos.x < DL.Instance.All.Width && wallPos.y >= 0 && wallPos.y < DL.Instance.All.Height))
            return;

        if (ActiveWall.ContainsKey(wallPos))
        {
            Debug.Log(wallPos);
            Pool.Return(ActiveWall[wallPos]);
            ActiveWall.Remove(wallPos);
            Addressables.Release(_handleDic[wallPos]);
            _handleDic.Remove(wallPos);
        }
    }

    public int Prime => (int)TileMapBasePrimeEnum.WallManager;
}
