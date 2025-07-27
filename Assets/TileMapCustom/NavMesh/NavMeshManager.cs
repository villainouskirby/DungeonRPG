using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using CM = ChunkManager;
using DL = DataLoader;

public class NavMeshManager : MonoBehaviour, ITileMapBase
{
    public Dictionary<Vector2Int, NavMeshDataInstance> ActiveNav;
    private Dictionary<Vector2Int, AsyncOperationHandle> _handleDic;
    public Dictionary<Vector2Int, NavMeshData> ForHelper;
    private MapEnum _currentMapType;

    public void Init()
    {
        _handleDic = new();
        ActiveNav = new();
        ForHelper = new();
    }

    public void InitMap(MapEnum mapType)
    {
        List<Vector2Int> target = new();
        foreach (var pair in ActiveNav)
            target.Add(pair.Key);
        for (int i = 0; i < target.Count; i++)
            UnLoadNav(target[i]);

        CM.Instance.ChunkLoadAction += LoadNav;
        CM.Instance.ChunkUnloadAction += UnLoadNav;

        _currentMapType = mapType;
    }

    public void StartMap(MapEnum mapType)
    {
        InitNav();
    }

    public void InitNav()
    {
        foreach (var key in CM.Instance.LoadedChunkIndex.Keys)
        {
            LoadNav(key);
        }
    }

    private void LoadNav(Vector2Int chunkPos)
    {
        if (!(chunkPos.x >= 0 && chunkPos.x < DL.Instance.All.Width && chunkPos.y >= 0 && chunkPos.y < DL.Instance.All.Height))
            return;

        _handleDic[chunkPos] = Addressables.LoadAssetAsync<NavMeshData>($"{_currentMapType.ToString()}_ChunkNavMesh_{chunkPos.x}_{chunkPos.y}");

        _handleDic[chunkPos].Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                SetNav((NavMeshData)op.Result, chunkPos);
        };
    }

    private void SetNav(NavMeshData data, Vector2Int chunkPos)
    {
        Vector3 pos = new Vector3(chunkPos.x * 16 + 8, chunkPos.y * 16 + 8, 0f);
        var inst = NavMesh.AddNavMeshData(data, pos, Quaternion.identity);
        ActiveNav[chunkPos] = inst;
        Debug.Log(data.name);
        ForHelper[chunkPos] = data;
    }

    private void UnLoadNav(Vector2Int navPos)
    {
        if (!(navPos.x >= 0 && navPos.x < DL.Instance.All.Width && navPos.y >= 0 && navPos.y < DL.Instance.All.Height))
            return;

        if (ActiveNav.ContainsKey(navPos))
        {
            ActiveNav[navPos].Remove();
            ActiveNav.Remove(navPos);
            Addressables.Release(_handleDic[navPos]);
            _handleDic.Remove(navPos);
        }
    }

    public int Prime => (int)TileMapBasePrimeEnum.NavMeshManager;
}
