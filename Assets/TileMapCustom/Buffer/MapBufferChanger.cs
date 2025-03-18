using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapBufferChanger
{
    private static Dictionary<int, (Vector2Int range, Dictionary<int, int> groupData)> _xGroupedData;
    private static List<(int[] change, int start, int length)> _changeList;
    private static Dictionary<int, (Vector2Int range, Dictionary<int, int> groupData)> _yGroupedData = new();
    private static List<int> _change;

    static MapBufferChanger()
    {
        _yGroupedData = new();
        _xGroupedData = new();
        _changeList = new();
        _change = new(1000);
        for (int i = 0; i < 1000; i++)
            _change.Add(0);
    }

    /// <summary>
    /// Buffer를 생성한다.
    /// </summary>
    /// <param name="data">기반으로 생성할 맵 </param>
    /// <param name="buffer">값을 넣을 버퍼</param>
    /// <param name="headerSize">헤더의 크기 (명시적으로 지정해두기 위함)</param>
    /// <param name="headerData">헤더에 들어갈 데이터들</param>
    public static void SetDataBuffer(TileMapData data, ref GraphicsBuffer buffer, int headerSize, params int[] headerData)
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

    public static void SetDataBuffer(int[] data, ref GraphicsBuffer buffer, int headerSize, params int[] headerData)
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

    public static void SetDataBuffer(float[] data, ref GraphicsBuffer buffer, int headerSize, params float[] headerData)
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


    public static void ChangeMapDataByRow(TileMapData oriMapData, List<Vector3Int> yGroupedData, int value, GraphicsBuffer targetBuffer, int headerSize)
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

    public static void ChangeMapDataByColumn(TileMapData oriMapData, List<Vector3Int> xGroupedData, int value, GraphicsBuffer targetBuffer, int headerSize)
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

    // 안에서 자체 로직 처리를 통해 자동 관리 ( Row, Column단위로 작업 분류 )
    // oriMapdata 또한 변경해준다.
    public static void ChangeMapDataByRow(TileMapData oriMapData, List<Vector3Int> mapData, GraphicsBuffer targetBuffer, int headerSize)
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

    public static void ChangeMapDataByCloumn(TileMapData oriMapData, List<(int x, int y, int value)> mapData, GraphicsBuffer targetBuffer, int headerSize)
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
}
