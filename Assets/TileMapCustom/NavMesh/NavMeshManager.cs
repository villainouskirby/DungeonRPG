using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using CM = ChunkManager;
using DL = DataLoader;

public class NavMeshManager : MonoBehaviour, ITileMapBase
{
    public static NavMeshManager Instance { get {  return _instance; } }
    private static NavMeshManager _instance;

    public Dictionary<Vector2Int, NavMeshDataInstance> ActiveNav;
    public HashSet<Vector2Int> DontUnLoadChunk;
    private Dictionary<Vector2Int, AsyncOperationHandle> _handleDic;
    private MapEnum _currentMapType;
    private AsyncOperationHandle _linkHandle;
    private Dictionary<Vector2Int, List<NavMeshLinkInstance>> _chunkLinkInstance;
    private Dictionary<Vector2Int, List<Vector2Int>> _chunkLink;

    private AsyncOperationHandle _navMeshHandle;
    private NavMeshData _navMeshData;

    public void Init()
    {
        _instance = this;
        _handleDic = new();
        ActiveNav = new();
        _chunkLinkInstance = new();
        _chunkLink = new();
        DontUnLoadChunk = new();
    }

    public void InitMap(MapEnum mapType)
    {
        /*
        ResetMonsterChunkNav();
        List<Vector2Int> target = new();
        foreach (var pair in ActiveNav)
            target.Add(pair.Key);
        for (int i = 0; i < target.Count; i++)
            UnLoadNav(target[i], true);

        CM.Instance.ChunkLoadAction += LoadNav;
        CM.Instance.ChunkUnloadAction += UnLoadNav;
        */

        NavMesh.RemoveAllNavMeshData();
        //Addressables.Release(_navMeshHandle);
        _currentMapType = mapType;

        _navMeshHandle = Addressables.LoadAssetAsync<NavMeshData>($"{_currentMapType.ToString()}_NavMesh");
        _navMeshHandle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _navMeshData = (NavMeshData)op.Result;
                NavMesh.AddNavMeshData(_navMeshData);
            }
        };

        /*
        if (_linkHandle.IsValid())
            Addressables.Release(_linkHandle);
        _linkHandle = Addressables.LoadAssetAsync<TextAsset>($"{mapType.ToString()}_NavChunkLinkData");
        _linkHandle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                _chunkLink = TypeByte2TypeConverter.Convert<Dictionary<Vector2Int, List<Vector2Int>>>(((TextAsset)op.Result).bytes);
        };
        */
    }

    public void StartMap(MapEnum mapType)
    {
        //InitNav();
    }

    public void InitNav()
    {
        foreach (var key in CM.Instance.LoadedChunkIndex.Keys)
        {
            LoadNav(key);
        }
    }

    public void SetMonsterChunkNav(List<Vector2Int> chunks)
    {
        ResetMonsterChunkNav();
        for (int i = 0; i < chunks.Count; i++)
            if (!DontUnLoadChunk.Contains(chunks[i]))
            {
                UnLoadNav(chunks[i], true);
                LoadNav(chunks[i]);
                DontUnLoadChunk.Add(chunks[i]);
            }
    }

    public void ResetMonsterChunkNav()
    {
        List<Vector2Int> remainMonsterChunk = DontUnLoadChunk.ToList();
        DontUnLoadChunk.Clear();
        for (int i = 0; i < remainMonsterChunk.Count; i++)
            { UnLoadNav(remainMonsterChunk[i], true); Debug.Log(remainMonsterChunk[i]); }
    }

    private void LoadNav(Vector2Int chunkPos)
    {
        return; // 꺼저잇


        // 네비매쉬 이슈로 청크 단위가 아닌 맵 전체 단위로 전환하면서 이제 더이상 청크 단위로 ~~ 안해도 된다.
        // 하지만 결합한 로직을 분리할 자신이 없으니 봉인해둔다.

        if (!(chunkPos.x >= 0 && chunkPos.x < DL.Instance.All.Width && chunkPos.y >= 0 && chunkPos.y < DL.Instance.All.Height))
            return;
        if (DontUnLoadChunk.Contains(chunkPos))
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
        if (ActiveNav.ContainsKey(chunkPos))
            return;
        Vector3 pos = new Vector3(chunkPos.x * 16 + 8, chunkPos.y * 16 + 8, 0f);
        ActiveNav[chunkPos] = NavMesh.AddNavMeshData(data);
        AddLink(chunkPos);
    }

    private void UnLoadAll()
    {

    }

    private void UnLoadNav(Vector2Int navPos) => UnLoadNav(navPos, false);

    private void UnLoadNav(Vector2Int navPos, bool forceDelete = false)
    {
        return; // 꺼저잇


        // 네비매쉬 이슈로 청크 단위가 아닌 맵 전체 단위로 전환하면서 이제 더이상 청크 단위로 ~~ 안해도 된다.
        // 하지만 결합한 로직을 분리할 자신이 없으니 봉인해둔다.

        if (!forceDelete && DontUnLoadChunk.Contains(navPos))
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
