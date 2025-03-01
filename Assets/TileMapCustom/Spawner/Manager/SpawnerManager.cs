using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SpawnerManager : MonoBehaviour
{
    public MapEnum MapType;

    public List<Spawner> AllSpawner;
    public HashSet<Spawner> ActiveSpawner;

    private SpawnerInfoData _spawnerInfoData;
    private float _tileSize;
    private GameObject _player;
    private Vector2Int _lastTile;

    void Awake()
    {
        SetDataAsset(MapType.ToString());
    }

    private void Start()
    {
        _tileSize = MapManager.Instance.TileSize;
        _player = MapManager.Instance.Player;
        _lastTile = new(0, 0);
        AllSpawner = new(1000);
        ActiveSpawner = new(1000);
        spawnerGizmo = new();
        Init();
    }

    void FixedUpdate()
    {
        _tileSize = MapManager.Instance.TileSize;
        Vector2Int newTile = GetCurrentTilePos();

        if (newTile != _lastTile)
        {
            CheckSpawner(newTile);
            _lastTile = newTile;
        }
        UpdateSpawner();
    }

    Vector2Int GetCurrentTilePos()
    {
        int tileX = Mathf.FloorToInt(_player.transform.position.x / _tileSize);
        int tileY = Mathf.FloorToInt(_player.transform.position.y / _tileSize);
        return new Vector2Int(tileX, tileY);
    }

    public void Init()
    {
        InitSpawner();
    }

    private void InitSpawner()
    {
        string assetName = MapType.ToString();
        string dataFilePath = $"{SpawnerExtractor.DataFileDirectory}{assetName}/{SpawnerExtractor.SpawnerFileDirectory}";

        foreach (var groupInfo in _spawnerInfoData.GroupInfo)
        {
            string[] groupData = groupInfo.Split("/");

            string group = groupData[0];
            string[] activeCase = groupData[1].Split("_");

            int selectCase = -1;
            string selectCaseName = "All";
            while (selectCaseName == "All")
            {
                selectCase = Random.Range(0, activeCase.Length);
                selectCaseName = activeCase[selectCase];

                if (activeCase.Length == 1)
                    break;
            }

            foreach(var case_ in activeCase)
            {
                if (case_ == "All" || case_ == selectCaseName)
                {
                    string path = $"{assetName}_{group}_{case_}";
                    SpawnerData caseSpawnerData = Instantiate(Resources.Load<SpawnerData>($"{dataFilePath}{path}"));
                    AllSpawner.AddRange(caseSpawnerData.Spawner);
                    foreach (var spawner in caseSpawnerData.Spawner)
                        spawner.Init();
                }
            }
        }
    }

    private void SetDataAsset(string assetName)
    {
        string dataFilePath = $"{SpawnerExtractor.DataFileDirectory}{assetName}/";
        _spawnerInfoData = Instantiate(Resources.Load<SpawnerInfoData>($"{dataFilePath}{assetName}SpawnerInfo"));
    }

    private void CheckSpawner(Vector2Int playerPos)
    {
        for(int i = 0; i < AllSpawner.Count; i++)
        {
            Spawner spawner = AllSpawner[i];
                
            Vector2Int spawnerTilePos = spawner.TilePos;
            int distance = Mathf.Max(Mathf.Abs(spawnerTilePos.x - playerPos.x), Mathf.Abs(spawnerTilePos.y - playerPos.y));

            if (distance <= spawner.MaxRange && distance > spawner.MinRange)
            {
                ActiveSpawner.Add(spawner);
                spawnerGizmo.Add(spawner);
            }
            else
            {
                ActiveSpawner.Remove(spawner);
                spawnerGizmo.Remove(spawner);
            }
        }
    }

    private void UpdateSpawner()
    {
        foreach(var spawner in ActiveSpawner)
        {
            spawner.TrySpawn();
        }
    }

    private HashSet<Spawner> spawnerGizmo;

    private void OnDrawGizmos()
    {
        if (spawnerGizmo == null)
            return;
        foreach(var a in spawnerGizmo)
        {
            Vector2Int tilePos = a.TilePos;
            Gizmos.color = Color.Lerp(Color.white, Color.red, a.CurrentTime / a.CoolTime);
            Gizmos.DrawCube(new(tilePos.x * _tileSize + _tileSize/2, tilePos.y * _tileSize + _tileSize/2, 0), Vector3.one * 1f);
        }
    }
}
