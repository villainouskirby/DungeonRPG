using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapVisitedChecker : MonoBehaviour
{
    [Header("Setting Values")]
    public int VisitedRange = 1;
    [Header("Target TileMap")]
    public TileMapController TargetTileMap;

    private bool _isCheck = false;

    private GameObject _player;
    private Vector2Int _lastTilePos;
    private Vector4Int _lastViewTilePos;
    private TileMapData _visitedMapData;
    private Camera _mainCamera;

    public void StartChecker()
    {
        Init();
        _isCheck = true;
    }

    private void Init()
    {
        _changeList = new();
        _visitedMapData = MapManager.Instance.VisitedMapData;
        _mainCamera = MapManager.Instance.TargetCamera;
        _lastViewTilePos = new(0, 0, 0, 0);
        _lastTilePos = new(0, 0);
        _player = MapManager.Instance.Player;
    }

    private void Update()
    {
        if (!_isCheck)
            return;

        CheckMove();
        CheckViewChange();
    }

    private void CheckMove()
    {
        float tileSize = MapManager.Instance.TileSize;
        Vector2Int newTilePos = GetCurrentTilePos(tileSize);

        if (newTilePos != _lastTilePos)
        {
            SetVisited(newTilePos);
            _lastTilePos = newTilePos;
        }
    }

    private void CheckViewChange()
    {
        float zDistance = Mathf.Abs(_mainCamera.transform.position.z);
        Vector2 bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector2 topRight = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, zDistance));

        Vector4Int newViewTilePos = new(
            Mathf.Clamp(Mathf.FloorToInt(bottomLeft.x), 0, _visitedMapData.Width - 1),
            Mathf.Clamp(Mathf.FloorToInt(topRight.x), 0, _visitedMapData.Width - 1),
            Mathf.Clamp(Mathf.FloorToInt(bottomLeft.y), 0, _visitedMapData.Height - 1),
            Mathf.Clamp(Mathf.FloorToInt(topRight.y), 0, _visitedMapData.Height - 1)
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

                if (correctX < 0 || correctX >= MapManager.Instance.VisitedMapData.Width || correctY < 0 || correctY >= MapManager.Instance.VisitedMapData.Height)
                    continue;

                if (_visitedMapData.GetTile(correctX, correctY) == 1)
                    continue;

                _changeList.Add(new(correctX, correctY, 1));
            }
        }

        MapManager.Instance.ChangeMapDataByRow(_visitedMapData, _changeList, MapManager.Instance.VisitedMapDataBufferRow, MapManager.Instance.VisitedMapDataBufferHeaderSize);
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

            MapManager.Instance.ChangeVisitedMapDataByRow(_visitedMapData, _changeList, 2, MapManager.Instance.VisitedMapDataBufferRow, MapManager.Instance.VisitedMapDataBufferHeaderSize);
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

            MapManager.Instance.ChangeVisitedMapDataByColumn(_visitedMapData, _changeList, 2, MapManager.Instance.VisitedMapDataBufferColumn, MapManager.Instance.VisitedMapDataBufferHeaderSize);
        }
    }

    Vector2Int GetCurrentTilePos(float tileSize)
    {
        int tileX = Mathf.FloorToInt(_player.transform.position.x / tileSize);
        int tileY = Mathf.FloorToInt(_player.transform.position.y / tileSize);
        return new(tileX, tileY);
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