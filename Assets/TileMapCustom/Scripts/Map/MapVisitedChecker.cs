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

    [HideInInspector]
    public bool IsCheck = false;

    private GameObject _player;
    private (int x, int y) _lastTilePos;
    private (int xMin, int xMax, int yMin, int yMax) _lastViewTilePos;
    private TileMapData _visitedMapData;
    private Camera _mainCamera;

    public void StartChecker()
    {
        Init();
        IsCheck = true;
    }

    private void Init()
    {
        _visitedMapData = MapManager.Instance.VisitedMapData;
        _mainCamera = MapManager.Instance.TargetCamera;
        _lastViewTilePos = (0, 0, 0, 0);
        _lastTilePos = (0, 0);
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if (!IsCheck)
            return;

        CheckMove();
        CheckViewChange();
    }

    private void CheckMove()
    {
        if (!IsCheck) return;
        float tileSize = MapManager.Instance.TileSize;
        (int x, int y) newTilePos = GetCurrentTilePos(tileSize);

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

        (int xMin, int xMax, int yMin, int yMax) newViewTilePos = (
            Mathf.Clamp(Mathf.FloorToInt(bottomLeft.x), 0, _visitedMapData.width - 1),
            Mathf.Clamp(Mathf.CeilToInt(topRight.x), 0, _visitedMapData.width - 1),
            Mathf.Clamp(Mathf.FloorToInt(bottomLeft.y), 0, _visitedMapData.height - 1),
            Mathf.Clamp(Mathf.CeilToInt(topRight.y), 0, _visitedMapData.height - 1)
            );

        if (newViewTilePos != _lastViewTilePos)
        {
            SetViewed(newViewTilePos);
            _lastViewTilePos = newViewTilePos;
        }
    }

    void SetVisited((int x, int y) point)
    {
        List<(int x, int y, int value)> changeList = new();

        for (int x = -VisitedRange; x <= VisitedRange; x++)
        {
            for (int y = -VisitedRange; y <= VisitedRange; y++)
            {
                int correctX = point.x + x;
                int correctY = point.y + y;

                if (correctX < 0 || correctX >= MapManager.Instance.VisitedMapData.width || correctY < 0 || correctY >= MapManager.Instance.VisitedMapData.height)
                    continue;

                if (_visitedMapData.GetTile(correctX, correctY) == 1)
                    continue;

                changeList.Add((correctX, correctY, 1));
            }
        }

        MapManager.Instance.ChangeMapDataByRow(_visitedMapData, changeList, MapManager.Instance.VisitedMapDataBufferRow, MapManager.Instance.VisitedMapDataBufferHeaderSize);
    }

    void SetViewed((int xMin, int xMax, int yMin, int yMax) newViewTilePos)
    {
        if(newViewTilePos.xMax - newViewTilePos.xMin > newViewTilePos.yMax - newViewTilePos.yMin)
        { // y 기준 행 정리
            List<(int y, int min, int max)> changeList = new();

            for (int y = newViewTilePos.yMin; y <= newViewTilePos.yMax; y++)
            {
                if (y < _lastViewTilePos.yMin) // 아래쪽
                {
                    changeList.Add((y, newViewTilePos.xMin, newViewTilePos.xMax));
                }
                else if(y > _lastViewTilePos.yMax) // 위쪽
                {
                    changeList.Add((y, newViewTilePos.xMin, newViewTilePos.xMax));
                }
                else // 양옆
                {
                    if (newViewTilePos.xMax > _lastViewTilePos.xMax)
                        changeList.Add((y, Mathf.Max(newViewTilePos.xMin, _lastViewTilePos.xMax + 1), newViewTilePos.xMax));
                    if (newViewTilePos.xMin < _lastViewTilePos.xMin)
                        changeList.Add((y, newViewTilePos.xMin, Mathf.Min(newViewTilePos.xMax, _lastViewTilePos.xMin - 1)));
                }
            }

            MapManager.Instance.ChangeVisitedMapDataByRow(_visitedMapData, changeList, 2, MapManager.Instance.VisitedMapDataBufferRow, MapManager.Instance.VisitedMapDataBufferHeaderSize);
        }
        else // True -> 가로가 더 길다. False -> 새로가 더 길다.
        { // x 기준 열 정리
            List<(int x, int min, int max)> changeList = new();

            for (int x = newViewTilePos.xMin; x <= newViewTilePos.xMax; x++)
            {
                if (x < _lastViewTilePos.xMin) // 왼쪽
                {
                    changeList.Add((x, newViewTilePos.yMin, newViewTilePos.yMax));
                }
                else if (x > _lastViewTilePos.xMax) // 오른쪽
                {
                    changeList.Add((x, newViewTilePos.yMin, newViewTilePos.yMax));
                }
                else // 위아래
                {
                    if (newViewTilePos.yMax > _lastViewTilePos.yMax)
                        changeList.Add((x, Mathf.Max(newViewTilePos.yMin, _lastViewTilePos.yMax + 1), newViewTilePos.yMax));
                    if (newViewTilePos.yMin < _lastViewTilePos.yMin)
                        changeList.Add((x, newViewTilePos.yMin, Mathf.Min(newViewTilePos.yMax, _lastViewTilePos.yMin - 1)));
                }
            }

            MapManager.Instance.ChangeVisitedMapDataByColumn(_visitedMapData, changeList, 2, MapManager.Instance.VisitedMapDataBufferColumn, MapManager.Instance.VisitedMapDataBufferHeaderSize);
        }
    }

    (int x, int y) GetCurrentTilePos(float tileSize)
    {
        int tileX = Mathf.FloorToInt(_player.transform.position.x / tileSize);
        int tileY = Mathf.FloorToInt(_player.transform.position.y / tileSize);
        return (tileX, tileY);
    }   
}
