using System.Collections;
using System.Collections.Generic;
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
    private TileMapData _visitedMapData;

    public void StartChecker()
    {
        Init();
        IsCheck = true;
    }

    private void Init()
    {
        _visitedMapData = MapManager.Instance.VisitedMapData;
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if (!IsCheck)
            return;

        float tileSize = MapManager.Instance.TileSize;
        (int x, int y) newTilePos = GetCurrentTilePos(tileSize);
        if (newTilePos != _lastTilePos)
        {
            SetVisited(newTilePos);
            _lastTilePos = newTilePos;
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

    void SetViewed((int x, int y) point)
    {

    }

    (int x, int y) GetCurrentTilePos(float tileSize)
    {
        int tileX = Mathf.FloorToInt(_player.transform.position.x / tileSize);
        int tileY = Mathf.FloorToInt(_player.transform.position.y / tileSize);
        return (tileX, tileY);
    }   
}
