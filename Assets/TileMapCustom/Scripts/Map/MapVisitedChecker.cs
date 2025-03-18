using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TM = TileMapMaster;
using MM = MapManager;
using static MapBufferChanger;
using System;

[RequireComponent(typeof(MapManager))]
public class MapVisitedChecker : MonoBehaviour, ITileMapOption
{
    [Header("Setting Values")]
    public int VisitedRange = 1;
    [Header("Target TileMap")]
    public TileMapController TargetTileMap;

    private bool _isActive = false;

    private Vector4Int _lastViewTilePos;


    public float GuideTileSize = 0.2f;
    // MapData
    private TileMapData _visitedMapData;

    // MapData property
    public TileMapData VisitedMapData => _visitedMapData;
    // MapDataBuffer
    private GraphicsBuffer _visitedMapDataBufferRow;
    private GraphicsBuffer _visitedMapDataBufferColumn;
    public readonly int VisitedMapDataBufferHeaderSize = 1;

    // MapDataBuffer Property
    public GraphicsBuffer VisitedMapDataBufferRow => _visitedMapDataBufferRow;
    public GraphicsBuffer VisitedMapDataBufferColumn => _visitedMapDataBufferColumn;


    public int Prime { get { return (int)TileMapOptionPrimeEnum.MapVisitedChecker; } }

    public void Init()
    {
        _changeList = new();
        _lastViewTilePos = new(0, 0, 0, 0);
    }
    public void InitMap(MapEnum mapType)
    {
        SetDataAsset(mapType.ToString());
        SetDataBuffers();
        _lastViewTilePos = new(TM.Instance.Player.transform.position, 0);
    }

    public void StartMap(MapEnum mapType)
    {
        OnOption();
    }


    public void OnOption()
    {
        if (_isActive)
            return;

        Shader.SetGlobalFloat("_VisitedActive", 1.0f);
        _isActive = true;
    }

    public void OffOption()
    {
        if (!_isActive)
            return;

        Shader.SetGlobalFloat("_VisitedActive", 0.0f);
        _isActive = false;
    }

    public TileMapOptionEnum OptionType { get { return TileMapOptionEnum.MapVisitedChecker; } }

    private void Update()
    {
        if (!_isActive)
            return;

        CheckViewChange();
    }

    private void SetDataAsset(string assetName)
    {
        string dataFilePath = $"{TileMapExtractor.DataFileDirectory}{assetName}/";
        _visitedMapData = Instantiate(Resources.Load<TileMapData>($"{dataFilePath}{assetName}Visited"));
    }

    /// <summary>
    /// 현재 Map Asset Data를 기반으로 Buffer들을 생성한다.
    /// </summary>
    private void SetDataBuffers()
    {
        SetDataBuffer(_visitedMapData, ref _visitedMapDataBufferRow, VisitedMapDataBufferHeaderSize,
            0);
        SetDataBuffer(_visitedMapData.GetColumnData(), ref _visitedMapDataBufferColumn, VisitedMapDataBufferHeaderSize,
            0);
    }


    private void CheckViewChange()
    {
        float zDistance = Mathf.Abs(TM.Instance.TargetCamera.transform.position.z);
        Vector2 bottomLeft = TM.Instance.TargetCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector2 topRight = TM.Instance.TargetCamera.ViewportToWorldPoint(new Vector3(1, 1, zDistance));

        Vector4Int newViewTilePos = new(
            Mathf.Clamp(Mathf.FloorToInt(bottomLeft.x), 0, MM.Instance.VisitedMapData.Width - 1),
            Mathf.Clamp(Mathf.FloorToInt(topRight.x), 0, MM.Instance.VisitedMapData.Width - 1),
            Mathf.Clamp(Mathf.FloorToInt(bottomLeft.y), 0, MM.Instance.VisitedMapData.Height - 1),
            Mathf.Clamp(Mathf.FloorToInt(topRight.y), 0, MM.Instance.VisitedMapData.Height - 1)
            );

        if (newViewTilePos != _lastViewTilePos)
        {
            SetViewed(newViewTilePos);
            _lastViewTilePos = newViewTilePos;
        }
    }

    /*
       x = x
       y = y
       z = value
    */

    private List<Vector3Int> _changeList;

    void SetVisited(Vector2Int point)
    {
        _changeList.Clear();

        for (int x = -VisitedRange; x <= VisitedRange; x++)
        {
            for (int y = -VisitedRange; y <= VisitedRange; y++)
            {
                int correctX = point.x + x;
                int correctY = point.y + y;

                if (correctX < 0 || correctX >= MM.Instance.VisitedMapData.Width || correctY < 0 || correctY >= MM.Instance.VisitedMapData.Height)
                    continue;

                if (MM.Instance.VisitedMapData.GetTile(correctX, correctY) == 1)
                    continue;

                _changeList.Add(new(correctX, correctY, 1));
            }
        }

       ChangeMapDataByRow(MM.Instance.VisitedMapData, _changeList, MM.Instance.VisitedMapDataBufferRow, MM.Instance.VisitedMapDataBufferHeaderSize);
    }

    /*
        xMin x
        xMax y
        yMin z
        yMax w
    */

    void SetViewed(Vector4Int newViewTilePos)
    {
        if(newViewTilePos.y - newViewTilePos.x > newViewTilePos.w - newViewTilePos.z)
        { // y 기준 행 정리
            _changeList.Clear();

            for (int y = newViewTilePos.z; y <= newViewTilePos.w; y++)
            {
                if (y < _lastViewTilePos.z) // 아래쪽
                {
                    _changeList.Add(new(y, newViewTilePos.x, newViewTilePos.y));
                }
                else if(y > _lastViewTilePos.w) // 위쪽
                {
                    _changeList.Add(new(y, newViewTilePos.x, newViewTilePos.y));
                }
                else // 양옆
                {
                    if (newViewTilePos.y > _lastViewTilePos.y)
                        _changeList.Add(new(y, Mathf.Max(newViewTilePos.x, _lastViewTilePos.y + 1), newViewTilePos.y));
                    if (newViewTilePos.x < _lastViewTilePos.x)
                        _changeList.Add(new(y, newViewTilePos.x, Mathf.Min(newViewTilePos.y, _lastViewTilePos.x - 1)));
                }
            }

            ChangeMapDataByRow(MM.Instance.VisitedMapData, _changeList, 2, MM.Instance.VisitedMapDataBufferRow, MM.Instance.VisitedMapDataBufferHeaderSize);
        }
        else // True -> 가로가 더 길다. False -> 새로가 더 길다.
        { // x 기준 열 정리
            _changeList.Clear();

            for (int x = newViewTilePos.x; x <= newViewTilePos.y; x++)
            {
                if (x < _lastViewTilePos.x) // 왼쪽
                {
                    _changeList.Add(new(x, newViewTilePos.z, newViewTilePos.w));
                }
                else if (x > _lastViewTilePos.y) // 오른쪽
                {
                    _changeList.Add(new(x, newViewTilePos.z, newViewTilePos.w));
                }
                else // 위아래
                {
                    if (newViewTilePos.w > _lastViewTilePos.w)
                        _changeList.Add(new(x, Mathf.Max(newViewTilePos.z, _lastViewTilePos.w + 1), newViewTilePos.w));
                    if (newViewTilePos.z < _lastViewTilePos.z)
                        _changeList.Add(new(x, newViewTilePos.z, Mathf.Min(newViewTilePos.w, _lastViewTilePos.z - 1)));
                }
            }

            ChangeMapDataByColumn(MM.Instance.VisitedMapData, _changeList, 2, MM.Instance.VisitedMapDataBufferColumn, MM.Instance.VisitedMapDataBufferHeaderSize);
        }
    }
}


public struct Vector4Int
{
    public int x;
    public int y;
    public int z;
    public int w;

    public Vector4Int(int x, int y, int z, int w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Vector4Int(Vector3 vector3, int w)
    {
        this.x = (int)vector3.x;
        this.y = (int)vector3.y;
        this.z = (int)vector3.z;
        this.w = w;
    }

     public static bool operator ==(Vector4Int lhs, Vector4Int rhs)
    {
        return lhs.x == rhs.x &&
               lhs.y == rhs.y &&
               lhs.z == rhs.z &&
               lhs.w == rhs.w;
    }

    // != 연산자 오버로딩
    public static bool operator !=(Vector4Int lhs, Vector4Int rhs)
    {
        return !(lhs == rhs);
    }
}