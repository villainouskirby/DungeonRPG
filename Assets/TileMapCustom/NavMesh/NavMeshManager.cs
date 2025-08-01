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
    private MapEnum _currentMapType;
    private AsyncOperationHandle _linkHandle;
    private Dictionary<Vector2Int, List<NavMeshLinkInstance>> _chunkLinkInstance;
    private Dictionary<Vector2Int, List<Vector2Int>> _chunkLink;

    public void Init()
    {
        _handleDic = new();
        ActiveNav = new();
        _chunkLinkInstance = new();
        _chunkLink = new();
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

        if (_linkHandle.IsValid())
            Addressables.Release(_linkHandle);
        _linkHandle = Addressables.LoadAssetAsync<TextAsset>($"{mapType.ToString()}_NavChunkLinkData");
        _linkHandle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                _chunkLink = TypeByte2TypeConverter.Convert<Dictionary<Vector2Int, List<Vector2Int>>>(((TextAsset)op.Result).bytes);
        };
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
        ActiveNav[chunkPos] = NavMesh.AddNavMeshData(data);
        AddLink(chunkPos);
    }

    private void UnLoadNav(Vector2Int navPos)
    {
        if (!(navPos.x >= 0 && navPos.x < DL.Instance.All.Width && navPos.y >= 0 && navPos.y < DL.Instance.All.Height))
            return;

        if (ActiveNav.ContainsKey(navPos))
        {
            NavMesh.RemoveNavMeshData(ActiveNav[navPos]);
            ActiveNav.Remove(navPos);
            Addressables.Release(_handleDic[navPos]);
            _handleDic.Remove(navPos);
        }

        if (_chunkLinkInstance.ContainsKey(navPos))
        {
            List<NavMeshLinkInstance> linkData = _chunkLinkInstance[navPos];
            for (int i = 0; i < linkData.Count; i++)
                NavMesh.RemoveLink(linkData[i]);
        }
    }

    private void AddLink(Vector2Int chunkPos)
    {
        if (!_chunkLink.ContainsKey(chunkPos))
            return;
        List<Vector2Int> pos = _chunkLink[chunkPos];
        List<NavMeshLinkInstance> links = new();

        for (int i = 0; i < pos.Count; i++)
        {
            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            Vector3 worldPos = new(pos[i].x + 0.5f + chunkPos.x * 16, pos[i].y + 0.5f + chunkPos.y * 16 );
            Debug.Log(pos[i]);
            var linkData = new NavMeshLinkData
            {
                startPosition = worldPos,
                endPosition = worldPos,
                width = 0.2f,
                bidirectional = true,
                area = 0,
                agentTypeID = buildSettings.agentTypeID,
            };

            links.Add(NavMesh.AddLink(linkData));
        }
        _chunkLinkInstance[chunkPos] = links;
    }

    public int Prime => (int)TileMapBasePrimeEnum.NavMeshManager;
}
