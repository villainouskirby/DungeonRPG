using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WallColliderManager : MonoBehaviour
{
    public static WallColliderManager Instance { get { return _instance; } }
    public static WallColliderManager _instance;

    private Dictionary<Vector2Int, GameObject> _activeCollider = new(); // 활성화 된 타일에 할당 되어 있는 오브젝트
    private HashSet<Vector2Int> _activeTile = new(); // 활성화 되어 있는 타일
    private Dictionary<Vector2Int, HashSet<Vector2Int>> _relativeTile = new(); // 활성화 되어 있는 타일에 연관되어있는 기준 타일 수
    private Dictionary<Vector2Int, Dictionary<int, int>> _activeTileRangeCnt = new(); // 해당 기준 타일에 어떤 범위가 활성화 되어 있는지
    private TileMapData _mapData;
    private float _tileSize;


    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _mapData = MapManager.Instance.MapData;
        _activeCollider = new();
        _activeTile = new();
        _relativeTile = new();
        _activeTileRangeCnt = new();
    }

    public void UpdateActiveTile(Vector2Int oldTargetTile, int oldRange, Vector2Int newTargetTile, int newRange)
    {
        _tileSize = MapManager.Instance.TileSize;

        if (newRange != 0 && newTargetTile.x >= 0 && newTargetTile.y >= 0 && newTargetTile.x < _mapData.width && newTargetTile.y < _mapData.height)
        {
            if (!_activeTileRangeCnt.ContainsKey(newTargetTile))
                _activeTileRangeCnt[newTargetTile] = new();
            var newTile = _activeTileRangeCnt[newTargetTile];
            if (!newTile.ContainsKey(newRange))
                newTile[newRange] = 0;
            newTile[newRange] += 1;
            AddActiveTile(newTargetTile, newTile);
        }

        if (oldRange != 0 && oldTargetTile.x >= 0 && oldTargetTile.y >= 0 && oldTargetTile.x < _mapData.width && oldTargetTile.y < _mapData.height)
        {
            if (!_activeTileRangeCnt.ContainsKey(oldTargetTile))
                _activeTileRangeCnt[oldTargetTile] = new();
            var oldTile = _activeTileRangeCnt[oldTargetTile];
            if (!oldTile.ContainsKey(oldRange))
                oldTile[oldRange] = 0;
            if (oldTile[oldRange] > 0)
                oldTile[oldRange] -= 1;
            if (oldTile[oldRange] == 0)
                DeleteActiveTile(oldTargetTile, oldTile, oldRange);
        }
    }

    private void AddActiveTile(Vector2Int tilePos, Dictionary<int, int> tileRange)
    {
        int maxRange = -1;

        foreach(var rangePair in tileRange)
        {
            if(rangePair.Value > 0)
                maxRange = Mathf.Max(maxRange, rangePair.Key);  
        }

        HashSet<Vector2Int> addTile = new();

        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                int correctX = tilePos.x + x;
                int correctY = tilePos.y + y;

                if (correctX < 0 || correctY < 0) continue;
                if (correctX >= _mapData.width || correctY >= _mapData.height) continue;
                if (MapManager.Instance.WallTileType.Contains(_mapData.GetTile(correctX, correctY)))
                    addTile.Add(new Vector2Int(correctX, correctY));
            }
        }

        HashSet<Vector2Int> tileToRemove = new();
        foreach (var tile in addTile)
        {
            if (!_relativeTile.ContainsKey(tile))
                _relativeTile[tile] = new();
            _relativeTile[tile].Add(tilePos);
            if (_activeTile.Contains(tile))
                tileToRemove.Add(tile);
        }
        addTile.ExceptWith(tileToRemove);


        foreach (var add in addTile)
        {
            GameObject wall = TileMapColliderObjPool.Instance.GetCollider();
            wall.transform.localScale = new Vector2(_tileSize, _tileSize);
            wall.transform.position = new Vector2(_tileSize * (add.x + 0.5f), _tileSize * (add.y + 0.5f));
            _activeCollider[add] = wall;
            _activeTile.Add(add);
            wall.transform.SetParent(MapManager.Instance.WallRoot.transform, true);
        }
    }

    private void DeleteActiveTile(Vector2Int tilePos, Dictionary<int, int> tileRange, int deleteRange)
    {
        int maxRange = -1;

        foreach (var rangePair in tileRange)
        {
            if (rangePair.Value > 0)
                maxRange = Mathf.Max(maxRange, rangePair.Key);
        }

        if (maxRange > deleteRange)
            return; // 더 큰 범위가 활성화 되어 있기에 그냥 return
        // range 크기는 이미 다 없다는 가정

        HashSet<Vector2Int> deleteTile = new();

        for (int x = -deleteRange; x <= deleteRange; x++)
        {
            for (int y = -deleteRange; y <= deleteRange; y++)
            {
                if (x >= -maxRange && x <= maxRange && y >= -maxRange && y <= maxRange)
                    continue;

                int correctX = tilePos.x + x;
                int correctY = tilePos.y + y;

                if (correctX < 0 || correctY < 0) continue;
                if (correctX >= _mapData.width || correctY >= _mapData.height) continue;

                if (MapManager.Instance.WallTileType.Contains(_mapData.GetTile(correctX, correctY)))
                    deleteTile.Add(new Vector2Int(correctX, correctY));
            }
        }

        HashSet<Vector2Int> tileToRemove = new();
        foreach (var tile in deleteTile)
        {
            _relativeTile[tile].Remove(tilePos);
            if (_relativeTile[tile].Count != 0)
            {
                tileToRemove.Add(tile); // 연관된 타일이 따로 존재하기에 지우면 안된다.
            }
        }
        deleteTile.ExceptWith(tileToRemove);

        foreach (var delete in deleteTile)
        {
            GameObject wall = _activeCollider[delete];
            TileMapColliderObjPool.Instance.ReturnCollider(wall);
            _activeCollider[delete] = null;
            _activeCollider.Remove(delete);
            _activeTile.Remove(delete);
        }
    }
}
