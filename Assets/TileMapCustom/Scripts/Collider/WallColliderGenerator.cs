using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WallColliderGenerator : MonoBehaviour
{
    [Header("Active Area Settings")]
    public int ActiveAreaSize = 1;

    [Header("Wall Settings")]
    public bool UseGenericWall = true;
    public int[] CustomWallTileType = new int[]
    {
        -1,
        1
    };
    private int[] _wallTileType;


    private TileMapData _mapData;
    private Dictionary<Vector2Int, GameObject> _activeCollider = new();
    private HashSet<Vector2Int> _lastTargetCollider = new();
    private (int x, int y) _lastTilePos;
    private float _tileSize;

    void Start()
    {
        _activeCollider = new();
        _lastTargetCollider = new();
        _lastTilePos = (0, 0);
        _mapData = MapManager.Instance.MapData;

        if (UseGenericWall)
            _wallTileType = MapManager.Instance.GenericWallTileType;
        else
            _wallTileType = CustomWallTileType;
    }

    void FixedUpdate()
    {
        _tileSize = MapManager.Instance.TileSize;
        (int x, int y) _newTilePos = GetCurrentTilePos();

        if( _newTilePos != _lastTilePos )
        {
            UpdateCollider(_newTilePos);
            _lastTilePos = _newTilePos;
        }
    }

    private void UpdateCollider((int x, int y) tilePos)
    {
        HashSet<Vector2Int> newTargetCollider = new();

        for(int x = -ActiveAreaSize; x <= ActiveAreaSize; x++)
        {
            for(int y = -ActiveAreaSize; y <= ActiveAreaSize; y++)
            {
                int correctX = tilePos.x + x;
                int correctY = tilePos.y + y;

                if(correctX < 0 || correctY < 0) continue;
                if(correctX >= _mapData.width || correctY >= _mapData.height) continue;
                if(_wallTileType.Contains(_mapData.GetTile(correctX, correctY)))
                    newTargetCollider.Add(new Vector2Int(correctX, correctY));
            }
        }

        HashSet<Vector2Int> addCollider = new(newTargetCollider);
        addCollider.ExceptWith(_lastTargetCollider);

        HashSet<Vector2Int> deleteCollider = new(_lastTargetCollider);
        deleteCollider.ExceptWith(newTargetCollider);

        foreach (var deletePos in deleteCollider)
        {
            if (_activeCollider.TryGetValue(deletePos, out GameObject returnObj))
            {
                TileMapColliderObjPool.Instance.ReturnCollider(returnObj);
                _activeCollider.Remove(deletePos);
            }
        }

        foreach (var addPos in addCollider)
        {
            GameObject wall = TileMapColliderObjPool.Instance.GetCollider();
            wall.transform.localScale = new Vector2(_tileSize, _tileSize);
            wall.transform.position = new Vector2(_tileSize * addPos.x + _tileSize * 0.5f, _tileSize * addPos.y + _tileSize * 0.5f);
            _activeCollider[addPos] = wall;
            wall.transform.SetParent(MapManager.Instance.WallRoot.transform, true);
        }

        _lastTargetCollider.Clear();
        _lastTargetCollider.UnionWith(newTargetCollider);
    }

    (int x, int y) GetCurrentTilePos()
    {
        int tileX = Mathf.FloorToInt(transform.position.x / _tileSize);
        int tileY = Mathf.FloorToInt(transform.position.y / _tileSize);
        return (tileX, tileY);
    }
}
