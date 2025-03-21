using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TM = TileMapMaster;
using static MapBufferChanger;

public class MapManager : MonoBehaviour, ITileMapBase
{
    public static MapManager Instance { get { return _instance; } }
    private static MapManager _instance;

    [Header("Wall Settings")]
    public int[] WallTileType = new int[0];
    public Dictionary<int, bool> _wallTileType;
    public GameObject WallRoot;

    // Base
    public int Prime { get { return (int)TileMapBasePrimeEnum.MapManager; } } 

    public void Init()
    {
        _instance = this;
        _wallTileType = new();
        for (int i = 0; i < WallTileType.Length; i++)
        {
            _wallTileType[WallTileType[i]] = true;
        }

        _mapVisitedChecker = gameObject.GetComponent<MapVisitedChecker>();
        _fovCaster = gameObject.GetComponent<FOVCaster>();
    }

    public void InitMap(MapEnum mapType)
    {
        SetDataAsset(mapType.ToString());
        SetDataBuffers();
    }

    public void StartMap(MapEnum type)
    {
        InitMap(type);
        TM.Instance.Player.transform.position = _mapData.PlayerSpawnPos;
    }

    [Header("Tile Settings")]
    public float TileSize = 1f;
    public float GuideTileSize = 0.2f;


    // MapData
    private TileMapData _mapData;
    private TileMapData _visitedMapData;

    // MapData property
    public TileMapData MapData => _mapData;
    public TileMapData VisitedMapData => _visitedMapData;
    // MapDataBuffer

    private GraphicsBuffer _mapDataBuffer;
    public readonly int MapDataBufferHeaderSize = 3;
    private GraphicsBuffer _visitedMapDataBufferRow;
    private GraphicsBuffer _visitedMapDataBufferColumn;
    public readonly int VisitedMapDataBufferHeaderSize = 1;


    // MapDataBuffer Property
    public GraphicsBuffer MapDataBuffer => _mapDataBuffer;
    public GraphicsBuffer VisitedMapDataBufferRow => _visitedMapDataBufferRow;
    public GraphicsBuffer VisitedMapDataBufferColumn => _visitedMapDataBufferColumn;


    private MapVisitedChecker _mapVisitedChecker;

    public FOVCaster FOVCaster { get { return _fovCaster; } }
    private FOVCaster _fovCaster;

    private void Update()
    {
        Shader.SetGlobalVector("_TileMapTargetCamera", TM.Instance.TargetCamera.transform.position);
        Shader.SetGlobalVector("_PlayerPos", TM.Instance.Player.transform.position);
    }

    public bool CheckWall(int type)
    {
        _wallTileType.TryGetValue(type, out bool result);
        return result;
    }


    /// <summary>
    /// Map Asset Data를 Resources 파일에서 읽는다.
    /// </summary>
    /// <param name="assetName">읽어올 파일 이름</param>
    private void SetDataAsset(string assetName)
    {
        string dataFilePath = $"{TileMapExtractor.DataFileDirectory}{assetName}/";
        _mapData = Instantiate(Resources.Load<TileMapData>($"{dataFilePath}{assetName}"));
        _visitedMapData = Instantiate(Resources.Load<TileMapData>($"{dataFilePath}{assetName}Visited"));
    }

    /// <summary>
    /// 현재 Map Asset Data를 기반으로 Buffer들을 생성한다.
    /// </summary>
    private void SetDataBuffers()
    {
        SetDataBuffer(_mapData, ref _mapDataBuffer, MapDataBufferHeaderSize,
            _mapData.Width, _mapData.Height, _mapData.Width * _mapData.Height + MapDataBufferHeaderSize);
        SetDataBuffer(_visitedMapData, ref _visitedMapDataBufferRow, VisitedMapDataBufferHeaderSize,
            0);
        SetDataBuffer(_visitedMapData.GetColumnData(), ref _visitedMapDataBufferColumn, VisitedMapDataBufferHeaderSize,
            0);
    }

    public void ChangeMapDataAll(int[] mapData, GraphicsBuffer targetBuffer, int headerSize, params int[] headerData)
    {
        SetDataBuffer(mapData, ref targetBuffer, headerSize,
            headerData);
    }
}