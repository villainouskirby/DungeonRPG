using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TM = TileMapMaster;
using MM = MapManager;

public class PlayerMoveChecker : MonoBehaviour, ITileMapBase
{
    public static PlayerMoveChecker Instance { get { return _instance; } }
    private static PlayerMoveChecker _instance;


    public Vector2Int LastTilePos;
    public Vector2Int NewTilePos;

    private Action _checkEndAction;
    private Action<Vector2Int> _moveAction;
    private Action<Vector2Int> _moveEndAction;

    public void AddCheckEndAction(Action action)
    {
        _checkEndAction += action;
    }
    public void AddMoveAction(Action<Vector2Int> action)
    {
        _moveAction += action;
    }
    public void AddMoveEndAction(Action<Vector2Int> action)
    {
        _moveEndAction += action;
    }
    public void DeleteCheckEndAction(Action action)
    {
        _checkEndAction -= action;
    }
    public void DeleteMoveAction(Action<Vector2Int> action)
    {
        _moveAction -= action;
    }
    public void DeleteMoveEndAction(Action<Vector2Int> action)
    {
        _moveEndAction -= action;
    }

    public int Prime { get { return (int)TileMapBasePrimeEnum.PlayerMoveChecker; } }

    public void Init()
    {
        _instance = this;
        LastTilePos = new(0, 0);
        NewTilePos = new(0, 0);
    }

    public void InitMap(MapEnum mapType)
    {
        Vector2Int spawnTilePos = DataLoader.Instance.All.PlayerSpawnTilePos;
        Vector3 spawnPos = new(spawnTilePos.x * MM.Instance.TileSize, spawnTilePos.y * MM.Instance.TileSize, 0);
        TM.Instance.Player.transform.position = spawnPos;
        LastTilePos = GetCurrentTilePos();
        _checkEndAction = null;
        _moveAction = null;
        _moveEndAction = null;
    }

    public void StartMap(MapEnum type)
    {
        InitMap(type);
    }

    void FixedUpdate()
    {
        NewTilePos = GetCurrentTilePos();

        if (NewTilePos != LastTilePos)
        {
            _moveAction?.Invoke(NewTilePos);
            LastTilePos = NewTilePos;
            _moveEndAction?.Invoke(NewTilePos);
        }
        _checkEndAction?.Invoke();
    }

    Vector2Int GetCurrentTilePos()
    {
        int tileX = Mathf.FloorToInt(TM.Instance.Player.transform.position.x / MM.Instance.TileSize);
        int tileY = Mathf.FloorToInt(TM.Instance.Player.transform.position.y / MM.Instance.TileSize);
        return new Vector2Int(tileX, tileY);
    }
}
