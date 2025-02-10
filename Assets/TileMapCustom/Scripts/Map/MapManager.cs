using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Wall Settings")]
    public int[] GenericWallTileType = new int[0];
    public GameObject WallRoot;

    public static MapManager Instance { get { return _instance; } }
    private static MapManager _instance;


    private void Awake()
    {
        _instance = this;
        InitData();
    }

    [Header("Map Type")]
    public MapEnum MapType;

    [Header("Tile Settings")]
    public float TileSize = 1f;
    public float GuideTileSize = 0.2f;

    [Header("Camera Settings")]
    public Camera TargetCamera;


    // MapData
    private TileMapData _mapData;
    private TileMapData _visitedMapData;
    private TileMapData _fovMapData;

    // MapData property
    public TileMapData MapData => _mapData;
    public TileMapData VisitedMapData => _visitedMapData;
    public TileMapData FOVMapData => _fovMapData;
    // MapDataBuffer

    private GraphicsBuffer _mapDataBuffer;
    public readonly int MapDataBufferHeaderSize = 3;
    private GraphicsBuffer _visitedMapDataBufferRow;
    private GraphicsBuffer _visitedMapDataBufferColumn;
    public readonly int VisitedMapDataBufferHeaderSize = 1;
    private GraphicsBuffer _fovMapDataBuffer;
    public readonly int FovMapDataBufferHeaderSize = 1;


    // MapDataBuffer Property
    public GraphicsBuffer MapDataBuffer => _mapDataBuffer;
    public GraphicsBuffer VisitedMapDataBufferRow => _visitedMapDataBufferRow;
    public GraphicsBuffer VisitedMapDataBufferColumn => _visitedMapDataBufferColumn;
    public GraphicsBuffer FOVMapDataBuffer => _fovMapDataBuffer;

    private MapVisitedChecker _mapVisitedChecker;

    private void Update()
    {
        Shader.SetGlobalVector("_TileMapTargetCamera", TargetCamera.transform.position);
    }

    /// <summary>
    /// 데이터를 세팅한다. ( MapType 기반 )
    /// </summary>
    public void InitData()
    {
        SetDataAsset(MapType.ToString());
        SetDataBuffers();

        _mapVisitedChecker = gameObject.GetComponent<MapVisitedChecker>();
        _mapVisitedChecker.StartChecker();
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
            _mapData.width, _mapData.height, _mapData.width * _mapData.height + MapDataBufferHeaderSize);
        SetDataBuffer(_visitedMapData, ref _visitedMapDataBufferRow, VisitedMapDataBufferHeaderSize,
            0);
        SetDataBuffer(_visitedMapData.GetColumnData(), ref _visitedMapDataBufferColumn, VisitedMapDataBufferHeaderSize,
            0);
        //SetDataBuffer(FOVMapData, FOVMapDataBuffer, 1);
    }

    /// <summary>
    /// Buffer를 생성한다.
    /// </summary>
    /// <param name="data">기반으로 생성할 맵 </param>
    /// <param name="buffer">값을 넣을 버퍼</param>
    /// <param name="headerSize">헤더의 크기 (명시적으로 지정해두기 위함)</param>
    /// <param name="headerData">헤더에 들어갈 데이터들</param>
    private void SetDataBuffer(TileMapData data, ref GraphicsBuffer buffer, int headerSize, params int[] headerData)
    {
        buffer?.Dispose();

        int bufferSize = data.width * data.height + headerSize;
        int[] _mapDataArray = new int[bufferSize];

        // Set Header Data
        for (int i = 0; i < headerSize; i++)
        {
            _mapDataArray[i] = headerData[i];
        }

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                _mapDataArray[headerSize + (y * data.width) + x] = data.GetTile(x, y);
            }
        }

        buffer = new(GraphicsBuffer.Target.Structured, bufferSize, sizeof(int));
        buffer.SetData(_mapDataArray);
    }

    private void SetDataBuffer(int[] data, ref GraphicsBuffer buffer, int headerSize, params int[] headerData)
    {
        buffer?.Dispose();

        int bufferSize = data.Length + headerSize;
;
        int[] _mapDataArray = new int[bufferSize];

        // Set Header Data
        for (int i = 0; i < headerSize; i++)
        {
            _mapDataArray[i] = headerData[i];
        }

        for (int i = 0; i < data.Length; i++)
        {
            _mapDataArray[headerSize + i] = data[i];
        }

        buffer = new(GraphicsBuffer.Target.Structured, bufferSize, sizeof(int));
        buffer.SetData(_mapDataArray);
    }

    // 안에서 자체 로직 처리를 통해 자동 관리 ( Row, Column단위로 작업 분류 )
    // oriMapdata 또한 변경해준다.
    public void ChangeMapDataByRow(TileMapData oriMapData, List<(int x,int y, int value)> mapData, GraphicsBuffer targetBuffer, int headerSize)
    {
        int xSize = oriMapData.width;

        Dictionary<int, ((int min, int max) range, Dictionary<int, int> groupData)> yGroupedData = new();

        for (int i = 0; i < mapData.Count; i++)
        {
            if (yGroupedData.TryGetValue(mapData[i].y, out var value))
            {
                yGroupedData[mapData[i].y].groupData[mapData[i].x] = mapData[i].value;
                yGroupedData[mapData[i].y] = ((Mathf.Min(value.range.min, mapData[i].x), Mathf.Max(value.range.max, mapData[i].x)), yGroupedData[mapData[i].y].groupData);
            }
            else
            {
                yGroupedData[mapData[i].y] = ((mapData[i].x, mapData[i].x), new() { { mapData[i].x, mapData[i].value } });
            }
        }


        List<(int[] change, int start, int length)> changeList = new();

        foreach (var yGroup in yGroupedData)
        {
            int y = yGroup.Key;
            var range = yGroup.Value.range;
            var groupData = yGroup.Value.groupData;

            int startIndex = y * xSize + range.min;
            int endIndex = y * xSize + range.max;

            int[] change = new int[endIndex - startIndex + 1];

            for (int i = range.min; i <= range.max; i++)
            {
                if (groupData.ContainsKey(i))
                {
                    oriMapData.SetTile(i, y, groupData[i]);
                    change[i - range.min] = groupData[i];
                }
                else
                    change[i - range.min] = oriMapData.GetTile(i, y);
            }

            changeList.Add((change, startIndex + headerSize, endIndex - startIndex + 1));
        }

        for (int i = 0; i < changeList.Count; i++)
        {
            targetBuffer.SetData(changeList[i].change, 0, changeList[i].start, changeList[i].length);
        }
    }

    public void ChangeMapDataByCloumn(TileMapData oriMapData, List<(int x, int y, int value)> mapData, GraphicsBuffer targetBuffer, int headerSize)
    {
        int ySize = oriMapData.height;

        Dictionary<int, ((int min, int max) range, Dictionary<int, int> groupData)> xGroupedData = new();

        for (int i = 0; i < mapData.Count; i++)
        {
            if (xGroupedData.TryGetValue(mapData[i].x, out var value))
            {
                xGroupedData[mapData[i].x].groupData[mapData[i].y] = mapData[i].value;
                xGroupedData[mapData[i].x] = ((Mathf.Min(value.range.min, mapData[i].y), Mathf.Max(value.range.max, mapData[i].y)), xGroupedData[mapData[i].x].groupData);
            }
            else
            {
                xGroupedData[mapData[i].x] = ((mapData[i].y, mapData[i].y), new() { { mapData[i].y, mapData[i].value } });
            }
        }


        List<(int[] change, int start, int length)> changeList = new();

        foreach (var xGroup in xGroupedData)
        {
            int x = xGroup.Key;
            var range = xGroup.Value.range;
            var groupData = xGroup.Value.groupData;

            int startIndex = x * ySize + range.min;
            int endIndex = x * ySize + range.max;

            int[] change = new int[endIndex - startIndex + 1];

            for (int i = range.min; i <= range.max; i++)
            {
                if (groupData.ContainsKey(i))
                {
                    oriMapData.SetTile(x, i, groupData[i]);
                    change[i - range.min] = groupData[i];
                }
                else
                    change[i - range.min] = oriMapData.GetTile(x, i);
            }

            changeList.Add((change, startIndex + headerSize, endIndex - startIndex + 1));
        }

        for (int i = 0; i < changeList.Count; i++)
        {
            targetBuffer.SetData(changeList[i].change, 0, changeList[i].start, changeList[i].length);
        }
    }

    public void ChangeVisitedMapDataByRow(TileMapData oriMapData, List<(int y, int min, int max)> yGroupedData, int value, GraphicsBuffer targetBuffer, int headerSize)
    {
        List<(int[] change, int start, int length)> changeList = new();

        int xSize = oriMapData.width;

        foreach (var yGroup in yGroupedData)
        {
            int y = yGroup.y;

            int startIndex = y * xSize + yGroup.min;
            int endIndex = y * xSize + yGroup.max;

            int[] change = new int[endIndex - startIndex + 1];
            Array.Fill(change, value);

            for (int x = yGroup.min; x <= yGroup.max; x++)
            {
                if (oriMapData.GetTile(x, y) == 1)
                    change[y * xSize + x - startIndex] = 1;
                else
                    oriMapData.SetTile(x, y, value);
            }

            changeList.Add((change, startIndex + headerSize, endIndex - startIndex + 1));
        }

        for (int i = 0; i < changeList.Count; i++)
        {
            targetBuffer.SetData(changeList[i].change, 0, changeList[i].start, changeList[i].length);
        }
    }

    public void ChangeVisitedMapDataByColumn(TileMapData oriMapData, List<(int x, int min, int max)> xGroupedData, int value, GraphicsBuffer targetBuffer, int headerSize)
    {
        List<(int[] change, int start, int length)> changeList = new();

        int ySize = oriMapData.height;

        foreach (var xGroup in xGroupedData)
        {
            int x = xGroup.x;

            int startIndex = x * ySize + xGroup.min;
            int endIndex = x * ySize + xGroup.max;

            int[] change = new int[endIndex - startIndex + 1];
            Array.Fill(change, value);

            for (int y = xGroup.min; y <= xGroup.max; y++)
            {
                if (oriMapData.GetTile(x, y) == 1)
                    change[x * ySize + y - startIndex] = 1;
                else
                    oriMapData.SetTile(x, y, value);
            }

            changeList.Add((change, startIndex + headerSize, endIndex - startIndex + 1));
        }

        for (int i = 0; i < changeList.Count; i++)
        {
            targetBuffer.SetData(changeList[i].change, 0, changeList[i].start, changeList[i].length);
        }
    }

    public void ChangeMapDataAll(int[] mapData, GraphicsBuffer targetBuffer, int headerSize, params int[] headerData)
    {
        SetDataBuffer(mapData, ref targetBuffer, headerSize,
            headerData);
    }
}