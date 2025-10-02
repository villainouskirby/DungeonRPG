using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using CM = ChunkManager;
using DL = DataLoader;

public class DecoManager : MonoBehaviour, ITileMapBase
{
    public static DecoManager Instance { get { return _instance; } }
    private static DecoManager _instance;

    public Dictionary<Vector2Int, List<DecoCom>> ActiveDecoObj;
    public DecoPool DecoPool;
    public GameObject ActiveRoot;
    public SpriteAtlas DecoAtlas;

    private AsyncOperationHandle<SpriteAtlas> _handle;

    public void Init()
    {
        _instance = this;

        DecoPool.Init();
        ActiveDecoObj = new();
    }

    public void InitMap(MapEnum mapType)
    {
        UnloadDecoAtlas();
        LoadDecoAtlas($"{mapType.ToString()}_DecoAtlas");

        foreach(var decoObj in ActiveDecoObj.Values)
        {
            for(int i = 0; i < decoObj.Count; i++)
            {
                DecoPool.Return(decoObj[i]);
            }
        }

        ActiveDecoObj = new();

        CM.Instance.ChunkLoadAction += LoadDecoInChunk;
        CM.Instance.ChunkUnloadAction += UnloadDecoInChunk;
    }

    public void StartMap(MapEnum mapType)
    {
        InitDeco();
    }

    public int Prime { get { return (int)TileMapBasePrimeEnum.DecoManager; } }

    public void LoadDecoAtlas(string address)
    {
        if (DecoAtlas != null) return;

        _handle = Addressables.LoadAssetAsync<SpriteAtlas>(address);
        DecoAtlas = _handle.WaitForCompletion();
    }

    public void UnloadDecoAtlas()
    {
        if (DecoAtlas == null)
            return;

        Addressables.Release(_handle);
        DecoAtlas = null;
    }


    // 맵 로드시 최초 1회
    public void InitDeco()
    {
        foreach(var key in CM.Instance.LoadedChunkIndex.Keys)
        {
            LoadDecoInChunk(key);
        }
        StartCoroutine(RefreshLight());
    }

    private IEnumerator RefreshLight()
    {
        yield return null;
        foreach (var decos in ActiveDecoObj.Values)
        {
            for (int i = 0; i < decos.Count; i++)
            {
                decos[i].Light.gameObject.SetActive(false);
                decos[i].Light.gameObject.SetActive(true);
            }
        }
    }

    public void LoadDecoInChunk(Vector2Int targetChunk)
    {
        Debug.Log(targetChunk);
        if (!DL.Instance.All.decoObjData.ContainsKey(targetChunk))
            return;

        var decoList = DL.Instance.All.decoObjData[targetChunk];

        if (!ActiveDecoObj.ContainsKey(targetChunk))
            ActiveDecoObj[targetChunk] = new();

        for (int i = 0; i < decoList.Count; i++)
        {
            // 세팅이 끝난 Deco가 나온다.
            DecoCom obj = DecoPool.Get(decoList[i]);

            obj.Sr.transform.SetParent(ActiveRoot.transform);
            ActiveDecoObj[targetChunk].Add(obj);
        }
    }

    public void UnloadDecoInChunk(Vector2Int targetChunk)
    {
        if (!ActiveDecoObj.ContainsKey(targetChunk))
            return;

        var chunkActiveDecoObj = ActiveDecoObj[targetChunk];

        for (int i = 0; i < chunkActiveDecoObj.Count; i++)
        {
            // 해당 청크에 위치하는 Deco는 전부 리턴한다.
            DecoPool.Return(chunkActiveDecoObj[i]);
        }

        ActiveDecoObj.Remove(targetChunk);
    }
}
