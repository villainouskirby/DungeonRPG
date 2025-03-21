using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using MM = MapManager;


public class WallColliderManager : MonoBehaviour, ITileMapOption
{
    public static WallColliderManager Instance { get { return _instance; } }
    public static WallColliderManager _instance;

    private Dictionary<Vector2Int, GameObject> _activeCollider = new(); // 활성화 된 타일에 할당 되어 있는 오브젝트
    private HashSet<Vector2Int> _activeTile = new(); // 활성화 되어 있는 타일
    private Dictionary<Vector2Int, HashSet<Vector2Int>> _relativeTile = new(); // 활성화 되어 있는 타일에 연관되어있는 기준 타일 수
    private Dictionary<Vector2Int, Dictionary<int, int>> _activeTileRangeCnt = new(); // 해당 기준 타일에 어떤 범위가 활성화 되어 있는지
    private bool _isActive = false;

    //Option

    public int Prime { get { return (int)TileMapOptionPrimeEnum.WallColliderManager; } }

    public void Init()
    {
        _instance = this;
        _activeCollider = new(225);
        _activeTile = new(225);
        _relativeTile = new(225);
        _activeTileRangeCnt = new(225);
        _deleteTile = new(225);
        _addTile = new(225);
        _deleteTileToRemove = new(225);
        _addTileToRemove = new(225);
    }
    public void InitMap(MapEnum mapType)
    {
        ResetCollider();

        _activeCollider.Clear();
        _activeTile.Clear();
        _relativeTile.Clear();
        _activeTileRangeCnt.Clear();
        _deleteTile.Clear();
        _addTile.Clear();
        _deleteTileToRemove.Clear();
        _addTileToRemove.Clear();
    }

    public void StartMap(MapEnum mapType)
    {
        InitMap(mapType);
    }


    public void OnOption()
    {
        if (_isActive)
            return;

        _isActive = true;
    }

    public void OffOption()
    {
        if (!_isActive)
            return;

        ResetCollider();
        _isActive = false;
    }

    private void ResetCollider()
    {
        foreach (var activeCollider in _activeCollider.Values)
        {
            TileMapColliderObjPool.Instance.ReturnCollider(activeCollider);
        }
    }

    public TileMapOptionEnum OptionType { get { return TileMapOptionEnum.WallColliderManager; } }


    public void UpdateActiveTile(Vector2Int oldTargetTile, int oldRange, Vector2Int newTargetTile, int newRange)
    {
        if (!_isActive)
            return;

        if (newRange != 0 && newTargetTile.x >= 0 && newTargetTile.y >= 0 && newTargetTile.x < MM.Instance.MapData.Width && newTargetTile.y < MM.Instance.MapData.Height)
        {
            if (!_activeTileRangeCnt.ContainsKey(newTargetTile))
                _activeTileRangeCnt[newTargetTile] = new();
            var newTile = _activeTileRangeCnt[newTargetTile];
            if (!newTile.ContainsKey(newRange))
                newTile[newRange] = 0;
            newTile[newRange] += 1;
            AddActiveTile(newTargetTile, newTile);
        }

        if (oldRange != 0 && oldTargetTile.x >= 0 && oldTargetTile.y >= 0 && oldTargetTile.x < MM.Instance.MapData.Width && oldTargetTile.y < MM.Instance.MapData.Height)
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

    private HashSet<Vector2Int> _addTile;
    private HashSet<Vector2Int> _addTileToRemove;

    private void AddActiveTile(Vector2Int tilePos, Dictionary<int, int> tileRange)
    {
        if (!_isActive)
            return;

        int maxRange = -1;

        foreach(var rangePair in tileRange)
        {
            if(rangePair.Value > 0)
                maxRange = Mathf.Max(maxRange, rangePair.Key);  
        }

        _addTile.Clear();

        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                int correctX = tilePos.x + x;
                int correctY = tilePos.y + y;

                if (correctX < 0 || correctY < 0) continue;
                if (correctX >= MM.Instance.MapData.Width || correctY >= MM.Instance.MapData.Height) continue;
                if (MM.Instance.CheckWall(MM.Instance.MapData.GetTile(correctX, correctY)))
                    _addTile.Add(new Vector2Int(correctX, correctY));
            }
        }

        _addTileToRemove.Clear();

        foreach (var tile in _addTile)
        {
            if (!_relativeTile.ContainsKey(tile))
                _relativeTile[tile] = new();
            _relativeTile[tile].Add(tilePos);
            if (_activeTile.Contains(tile))
                _addTileToRemove.Add(tile);
        }
        _addTile.ExceptWith(_addTileToRemove);


        foreach (var add in _addTile)
        {
            GameObject wall = TileMapColliderObjPool.Instance.GetCollider();
            wall.transform.localScale = new Vector2(MM.Instance.TileSize, MM.Instance.TileSize);
            wall.transform.position = new Vector2(MM.Instance.TileSize * (add.x + 0.5f), MM.Instance.TileSize * (add.y + 0.5f));
            _activeCollider[add] = wall;
            _activeTile.Add(add);
            wall.transform.SetParent(MM.Instance.WallRoot.transform, true);
        }
    }

    private HashSet<Vector2Int> _deleteTile;
    private HashSet<Vector2Int> _deleteTileToRemove;

    private void DeleteActiveTile(Vector2Int tilePos, Dictionary<int, int> tileRange, int deleteRange)
    {
        if (!_isActive)
            return;

        int maxRange = -1;

        foreach (var rangePair in tileRange)
        {
            if (rangePair.Value > 0)
                maxRange = Mathf.Max(maxRange, rangePair.Key);
        }

        if (maxRange > deleteRange)
            return; // 더 큰 범위가 활성화 되어 있기에 그냥 return
        // range 크기는 이미 다 없다는 가정

        _deleteTile.Clear();

        for (int x = -deleteRange; x <= deleteRange; x++)
        {
            for (int y = -deleteRange; y <= deleteRange; y++)
            {
                if (x >= -maxRange && x <= maxRange && y >= -maxRange && y <= maxRange)
                    continue;

                int correctX = tilePos.x + x;
                int correctY = tilePos.y + y;

                if (correctX < 0 || correctY < 0) continue;
                if (correctX >= MM.Instance.MapData.Width || correctY >= MM.Instance.MapData.Height) continue;

                if (MM.Instance.CheckWall(MM.Instance.MapData.GetTile(correctX, correctY)))
                    _deleteTile.Add(new Vector2Int(correctX, correctY));
            }
        }

        _deleteTileToRemove.Clear();
        foreach (var tile in _deleteTile)
        {
            _relativeTile[tile].Remove(tilePos);
            if (_relativeTile[tile].Count != 0)
            {
                _deleteTileToRemove.Add(tile); // 연관된 타일이 따로 존재하기에 지우면 안된다.
            }
        }
        _deleteTile.ExceptWith(_deleteTileToRemove);

        foreach (var delete in _deleteTile)
        {
            GameObject wall = _activeCollider[delete];
            TileMapColliderObjPool.Instance.ReturnCollider(wall);
            _activeCollider[delete] = null;
            _activeCollider.Remove(delete);
            _activeTile.Remove(delete);
        }
    }
}
