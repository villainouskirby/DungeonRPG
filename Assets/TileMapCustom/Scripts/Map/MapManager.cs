using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Wall Settings")]
    public int[] WallTileType = new int[0];
    public Dictionary<int, bool> _wallTileType;
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

    [Header("Player Settings")]
    public GameObject Player;


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

    // FOVDataBuffer
    private GraphicsBuffer _fovDataBuffer;
    public readonly int FovDataBufferHeaderSize = 1;


    // MapDataBuffer Property
    public GraphicsBuffer MapDataBuffer => _mapDataBuffer;
    public GraphicsBuffer VisitedMapDataBufferRow => _visitedMapDataBufferRow;
    public GraphicsBuffer VisitedMapDataBufferColumn => _visitedMapDataBufferColumn;

    // FOVDataBuffer Property
    public GraphicsBuffer FOVDataBuffer => _fovDataBuffer;

    private MapVisitedChecker _mapVisitedChecker;
    private FOVCaster _fovCaster;

    private void Update()
    {
        Shader.SetGlobalVector("_TileMapTargetCamera", TargetCamera.transform.position);
        Shader.SetGlobalVector("_PlayerPos", Player.transform.position);
        Shader.SetGlobalInt("_FOVRadius", _fovCaster.Radius);
    }

    public bool CheckWall(int type)
    {
        _wallTileType.TryGetValue(type, out bool result);
        return result;
    }

    /// <summary>
    /// 데이터를 세팅한다. ( MapType 기반 )
    /// </summary>
    public void InitData()
    {
        _yGroupedData = new();
        _xGroupedData = new();
        _changeList = new();
        _wallTileType = new();
        _change = new(1000);
        for (int i = 0; i < 1000; i++)
            _change.Add(0);
        for(int i = 0; i < WallTileType.Length; i++)
        {
            _wallTileType[WallTileType[i]] = true;
        }

        _mapVisitedChecker = gameObject.GetComponent<MapVisitedChecker>();
        _fovCaster = gameObject.GetComponent<FOVCaster>();

        SetDataAsset(MapType.ToString());
        SetDataBuffers();

        _mapVisitedChecker.StartChecker();
        _fovCaster.StartCast();
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

        int bufferSize = data.Width * data.Height + headerSize;
        int[] _mapDataArray = new int[bufferSize];

        // Set Header Data
        for (int i = 0; i < headerSize; i++)
        {
            _mapDataArray[i] = headerData[i];
        }

        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                _mapDataArray[headerSize + (y * data.Width) + x] = data.GetTile(x, y);
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

    private void SetDataBuffer(float[] data, ref GraphicsBuffer buffer, int headerSize, params float[] headerData)
    {
        buffer?.Dispose();

        int bufferSize = data.Length + headerSize;
        ;
        float[] _mapDataArray = new float[bufferSize];

        // Set Header Data
        for (int i = 0; i < headerSize; i++)
        {
            _mapDataArray[i] = headerData[i];
        }

        for (int i = 0; i < data.Length; i++)
        {
            _mapDataArray[headerSize + i] = data[i];
        }

        buffer = new(GraphicsBuffer.Target.Structured, bufferSize, sizeof(float));
        buffer.SetData(_mapDataArray);
    }

    private int _lastRadius;
    private Action _fovDataChangeAlarm;

    public void AddFOVDataChangeAction(Action action)
    {
        if (_fovDataChangeAlarm == null)
            _fovDataChangeAlarm = action;
        else
            _fovDataChangeAlarm += action;
    }

    public void ChangeFOVData(float[] fovData, int radius)
    {
        if (_lastRadius != radius)
        {
            SetDataBuffer(new float[(_fovCaster.Radius * 2 + 1) * (_fovCaster.Radius * 2 + 1)], ref _fovDataBuffer, FovDataBufferHeaderSize,
            0);
            _fovDataChangeAlarm?.Invoke();
        }
        _lastRadius = radius;
        FOVDataBuffer.SetData(fovData, 0, 1, fovData.Length);
    }

    private List<(int[] change, int start, int length)> _changeList;

    private Dictionary<int, (Vector2Int range, Dictionary<int, int> groupData)> _yGroupedData = new();

    // 안에서 자체 로직 처리를 통해 자동 관리 ( Row, Column단위로 작업 분류 )
    // oriMapdata 또한 변경해준다.
    public void ChangeMapDataByRow(TileMapData oriMapData, List<Vector3Int> mapData, GraphicsBuffer targetBuffer, int headerSize)
    {
        int xSize = oriMapData.Width;

        _yGroupedData.Clear();

        for (int i = 0; i < mapData.Count; i++)
        {
            if (_yGroupedData.TryGetValue(mapData[i].y, out var value))
            {
                _yGroupedData[mapData[i].y].groupData[mapData[i].x] = mapData[i].z;
                _yGroupedData[mapData[i].y] = (new(Mathf.Min(value.range.x, mapData[i].x), Mathf.Max(value.range.y, mapData[i].x)), _yGroupedData[mapData[i].y].groupData);
            }
            else
            {
                _yGroupedData[mapData[i].y] = (new(mapData[i].x, mapData[i].x), new() { { mapData[i].x, mapData[i].z } });
            }
        }


        _changeList.Clear();

        foreach (var yGroup in _yGroupedData)
        {
            int y = yGroup.Key;
            var range = yGroup.Value.range;
            var groupData = yGroup.Value.groupData;

            int startIndex = y * xSize + range.x;
            int endIndex = y * xSize + range.y;

            if (_change.Count < endIndex - startIndex + 1)
            {
                _change = new(endIndex - startIndex + 1);
                for (int i = 0; i < endIndex - startIndex + 1 - _change.Count; i++)
                    _change.Add(0);
            }

            for (int i = range.x; i <= range.y; i++)
            {
                if (groupData.ContainsKey(i))
                {
                    oriMapData.SetTile(i, y, groupData[i]);
                    _change[i - range.x] = groupData[i];
                }
                else
                    _change[i - range.x] = oriMapData.GetTile(i, y);
            }

            targetBuffer.SetData(_change, 0, startIndex + headerSize, endIndex - startIndex + 1);
        }
    }

    private Dictionary<int, (Vector2Int range, Dictionary<int, int> groupData)> _xGroupedData;

    public void ChangeMapDataByCloumn(TileMapData oriMapData, List<(int x, int y, int value)> mapData, GraphicsBuffer targetBuffer, int headerSize)
    {
        int ySize = oriMapData.Height;

        _xGroupedData.Clear();

        for (int i = 0; i < mapData.Count; i++)
        {
            if (_xGroupedData.TryGetValue(mapData[i].x, out var value))
            {
                _xGroupedData[mapData[i].x].groupData[mapData[i].y] = mapData[i].value;
                _xGroupedData[mapData[i].x] = (new(Mathf.Min(value.range.x, mapData[i].y), Mathf.Max(value.range.y, mapData[i].y)), _xGroupedData[mapData[i].x].groupData);
            }
            else
            {
                _xGroupedData[mapData[i].x] = (new(mapData[i].y, mapData[i].y), new() { { mapData[i].y, mapData[i].value } });
            }
        }


        _changeList.Clear();

        foreach (var xGroup in _xGroupedData)
        {
            int x = xGroup.Key;
            var range = xGroup.Value.range;
            var groupData = xGroup.Value.groupData;

            int startIndex = x * ySize + range.x;
            int endIndex = x * ySize + range.y;

            int[] change = new int[endIndex - startIndex + 1];

            for (int i = range.x; i <= range.y; i++)
            {
                if (groupData.ContainsKey(i))
                {
                    oriMapData.SetTile(x, i, groupData[i]);
                    change[i - range.x] = groupData[i];
                }
                else
                    change[i - range.x] = oriMapData.GetTile(x, i);
            }

            _changeList.Add((change, startIndex + headerSize, endIndex - startIndex + 1));
        }

        for (int i = 0; i < _changeList.Count; i++)
        {
            targetBuffer.SetData(_changeList[i].change, 0, _changeList[i].start, _changeList[i].length);
        }
    }

    private List<int> _change;

    public void ChangeVisitedMapDataByRow(TileMapData oriMapData, List<Vector3Int> yGroupedData, int value, GraphicsBuffer targetBuffer, int headerSize)
    {
        int xSize = oriMapData.Width;

        foreach (var yGroup in yGroupedData)
        {
            int y = yGroup.x;

            int startIndex = y * xSize + yGroup.y;
            int endIndex = y * xSize + yGroup.z;

            if (_change.Count < endIndex - startIndex + 1)
            {
                _change = new(endIndex - startIndex + 1);
                for (int i = 0; i < endIndex - startIndex + 1 - _change.Count; i++)
                    _change.Add(0);
            }
            for (int x = yGroup.y; x <= yGroup.z; x++)
            {
                if (oriMapData.GetTile(x, y) == 1)
                    _change[y * xSize + x - startIndex] = 1;
                else
                {
                    _change[y * xSize + x - startIndex] = value;
                    oriMapData.SetTile(x, y, value);
                }    
            }

            targetBuffer.SetData(_change, 0, startIndex + headerSize, endIndex - startIndex + 1);
        }
    }

    public void ChangeVisitedMapDataByColumn(TileMapData oriMapData, List<Vector3Int> xGroupedData, int value, GraphicsBuffer targetBuffer, int headerSize)
    {
        _changeList.Clear();

        int ySize = oriMapData.Height;

        foreach (var xGroup in xGroupedData)
        {
            int x = xGroup.x;

            int startIndex = x * ySize + xGroup.y;
            int endIndex = x * ySize + xGroup.z;

            int[] change = new int[endIndex - startIndex + 1];
            Array.Fill(change, value);

            for (int y = xGroup.y; y <= xGroup.z; y++)
            {
                if (oriMapData.GetTile(x, y) == 1)
                    change[x * ySize + y - startIndex] = 1;
                else
                    oriMapData.SetTile(x, y, value);
            }

            _changeList.Add((change, startIndex + headerSize, endIndex - startIndex + 1));
        }

        for (int i = 0; i < _changeList.Count; i++)
        {
            targetBuffer.SetData(_changeList[i].change, 0, _changeList[i].start, _changeList[i].length);
        }
    }

    public void ChangeMapDataAll(int[] mapData, GraphicsBuffer targetBuffer, int headerSize, params int[] headerData)
    {
        SetDataBuffer(mapData, ref targetBuffer, headerSize,
            headerData);
    }
}