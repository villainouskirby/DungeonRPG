using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TM = TileMapMaster;
using DL = DataLoader;
using CM = ChunkManager;
using static MapBufferChanger;
using System.Linq;
using Unity.VisualScripting;
using Unity.Mathematics;

public class MapManager : MonoBehaviour, ITileMapBase
{
    public static MapManager Instance { get { return _instance; } }
    private static MapManager _instance;

    // Base
    public int Prime { get { return (int)TileMapBasePrimeEnum.MapManager; } } 

    public void Init()
    {
        _instance = this;
    }

    public void InitMap(MapEnum mapType)
    {
        if (_controller != null)
        {
            for (int i = 0; i < _controller.Length; i++)
            {
                Destroy(_controller[i].gameObject);
            }
        }

        if (_layerBuffer != null)
        {
            for (int i = 0; i < _layerBuffer.Length; i++)
                _layerBuffer[i].Dispose();
        }

        _layerBuffer = new GraphicsBuffer[DL.Instance.All.LayerCount];
        _controller = new TileMapController[DL.Instance.All.LayerCount];


        for (int i = 0; i < DL.Instance.All.LayerCount; i++)
        {
            _controller[i] =
            Instantiate(TileMapPrefab, TM.Instance.LayerRoot.transform, false).GetComponent<TileMapController>();
            SetDataBuffer(ref _layerBuffer[i], i);
            _controller[i].InitTileMap(_layerBuffer[i]);
            SpriteRenderer rd = _controller[i].GetComponent<SpriteRenderer>();
            rd.sortingOrder = DL.Instance.All.TileMapLayerInfo[i].LayerIndex;
            _controller[i].transform.position = new(0, 0, DL.Instance.All.TileMapLayerInfo[i].Z);
        }

        SetMappingDataBuffer(CM.Instance.GetMappingArray(), ref _mappingBuffer);
    }

    public void ChangeMapping()
    {
        _mappingBuffer.SetData(CM.Instance.GetMappingArray());
    }

    public void ChangeMapData( int layer)
    {
        ChangeMapDataBuffer(_layerBuffer[layer]);
    }


    public void StartMap(MapEnum type)
    {
        InitMap(type);
        SetGlobal();
    }

    [Header("Tile Settings")]
    public float TileSize = 1f;
    public float GuideTileSize = 0.2f;


    private GraphicsBuffer _mapDataBuffer;
    private GraphicsBuffer _visitedMapDataBufferRow;
    private GraphicsBuffer _visitedMapDataBufferColumn;
    private GraphicsBuffer _mappingBuffer;

    private GraphicsBuffer[] _layerBuffer;
    private TileMapController[] _controller;

    [Header("TileMap Prefab")]
    public GameObject TileMapPrefab;



    // MapDataBuffer Property
    public GraphicsBuffer MapDataBuffer => _mapDataBuffer;
    public GraphicsBuffer VisitedMapDataBufferRow => _visitedMapDataBufferRow;
    public GraphicsBuffer VisitedMapDataBufferColumn => _visitedMapDataBufferColumn;
    public GraphicsBuffer MappingBuffer => _mappingBuffer;


    //private MapVisitedChecker _mapVisitedChecker;


    private void Update()
    {
        Shader.SetGlobalVector("_TileMapTargetCamera", TM.Instance.TargetCamera.transform.position);
        Shader.SetGlobalVector("_PlayerPos", TM.Instance.Player.transform.position);
    }

    private void SetGlobal()
    {
        Shader.SetGlobalTexture("_TileTexture", CreateTexture2DArray(DL.Instance.All.MapTexture));
        Shader.SetGlobalFloat("_TileSize", TileSize);
        Shader.SetGlobalInt("_TextureSize", DL.Instance.All.MapTexture.Length);
        Shader.SetGlobalInt("_ViewBoxSize", TM.Instance.ViewBoxSize);
        Shader.SetGlobalInt("_ViewChunkSize", CM.Instance.ViewChunkSize);
        Shader.SetGlobalInt("_ChunkSize", DL.Instance.All.ChunkSize);
        Shader.SetGlobalBuffer("_MappingBuffer", _mappingBuffer);
        Shader.SetGlobalInt("_CenterChunkX", CM.Instance.LastChunkPos.x);
        Shader.SetGlobalInt("_CenterChunkY", CM.Instance.LastChunkPos.y);
    }

    public Texture2DArray CreateTexture2DArray(Texture2D[] tileTexture)
    {
        int width = tileTexture[0].width;
        int height = tileTexture[0].height;
        TextureFormat format = tileTexture[0].format;
        bool mipChain = false;
        

        Texture2DArray tileTextureArray = new(width, height, tileTexture.Length, format, mipChain);
        tileTextureArray.anisoLevel = 1;
        tileTextureArray.filterMode = FilterMode.Point;
        tileTextureArray.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < tileTexture.Length; i++)
        {
            tileTexture[i].filterMode = FilterMode.Point;
            tileTexture[i].wrapMode = TextureWrapMode.Clamp;
            Graphics.CopyTexture(tileTexture[i], 0, 0, tileTextureArray, i, 0);
        }

        return tileTextureArray;
    }

    public bool CheckWall(Vector2Int tilePos)
    {
        for (int i = 0; i < DL.Instance.All.LayerCount; i++)
        {
            if (DL.Instance.All.TileMapLayerInfo[i].WallTileIndex.Contains(CM.Instance.GetTile(tilePos, i)))
                return true;
        }
        return false;
    }


    /// <summary>
    /// 현재 Map Asset Data를 기반으로 Buffer들을 생성한다.
    /// </summary>
    private void SetDataBuffer(ref GraphicsBuffer buffer, int layer)
    {
        CM.Instance.GetViewBoxData(layer);
        SetMapDataBuffer(ref buffer);
    }
}