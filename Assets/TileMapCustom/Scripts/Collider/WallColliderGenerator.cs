using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WallColliderGenerator : MonoBehaviour
{
    [Header("Active Settings")]
    public int ActiveRange = 1;

    private TileMapData _mapData;
    private (Vector2Int pos, int range) _lastTilePos;
    private float _tileSize;

    void Start()
    {
        _lastTilePos = (new Vector2Int(0, 0), 0);
        _mapData = MapManager.Instance.MapData;
    }

    void FixedUpdate()
    {
        _tileSize = MapManager.Instance.TileSize;
        (Vector2Int pos, int range) newTilePos = (GetCurrentTilePos(), ActiveRange);

        if( newTilePos != _lastTilePos )
        {
            WallColliderManager.Instance.UpdateActiveTile(_lastTilePos.pos, _lastTilePos.range, newTilePos.pos, newTilePos.range);
            _lastTilePos = newTilePos;
        }
    }

    Vector2Int GetCurrentTilePos()
    {
        int tileX = Mathf.FloorToInt(transform.position.x / _tileSize);
        int tileY = Mathf.FloorToInt(transform.position.y / _tileSize);
        return new Vector2Int(tileX, tileY);
    }

    private void OnDisable()
    {
        if(MapManager.Instance.WallRoot != null)
            WallColliderManager.Instance.UpdateActiveTile(_lastTilePos.pos, _lastTilePos.range, new Vector2Int(0, 0), 0);
    }
}
