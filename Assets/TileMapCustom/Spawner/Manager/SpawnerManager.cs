using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TM = TileMapMaster;
using MM = MapManager;
using DL = DataLoader;

public class SpawnerManager : MonoBehaviour, ITileMapOption
{
    public List<Spawner> AllSpawner;
    public HashSet<Spawner> ActiveSpawner;

    private bool _isActive;

    // Option
    public int Prime { get { return (int)TileMapOptionPrimeEnum.SpawnerManager; } }

    public void Init()
    {
        AllSpawner = new(1000);
        ActiveSpawner = new(1000);

#if UNITY_EDITOR
        spawnerGizmo = new();
#endif
    }

    public void InitMap(MapEnum mapType)
    {
        ResetSpawnwer();
        AllSpawner.Clear();
        ActiveSpawner.Clear();
        SetSpawner(mapType.ToString());
    }

    public void StartMap(MapEnum mapType)
    {
        InitMap(mapType);
        CheckSpawner(PlayerMoveChecker.Instance.LastTilePos);
    }

    public void OnOption()
    {
        if (_isActive)
            return;

        PlayerMoveChecker.Instance.AddMoveAction(CheckSpawner);
        PlayerMoveChecker.Instance.AddCheckEndAction(UpdateSpawner);
        _isActive = true;
    }

    public void OffOption()
    {
        if (!_isActive)
            return;

        PlayerMoveChecker.Instance.DeleteMoveAction(CheckSpawner);
        PlayerMoveChecker.Instance.DeleteCheckEndAction(UpdateSpawner);
        ResetSpawnwer();
        AllSpawner.Clear();
        ActiveSpawner.Clear();
        _isActive = false;
    }

    private void ResetSpawnwer()
    {
        for (int i = 0; i < AllSpawner.Count; i++)
        {
            if (AllSpawner[i].SpawnObj != null)
            {
                FarmableBase targetFarm = AllSpawner[i].SpawnObj.GetComponent<FarmableBase>();
                switch (targetFarm.Type)
                {
                    case FarmEnum.Plant:
                        SpawnerPool.Instance.PlantPool.Release(targetFarm.PlantType, AllSpawner[i].SpawnObj);
                        break;
                    case FarmEnum.Mineral:
                        SpawnerPool.Instance.MineralPool.Release(targetFarm.MineralType, AllSpawner[i].SpawnObj);
                        break;
                }
            }
        }

        AllSpawner.Clear();
        ActiveSpawner.Clear();
    }

    public TileMapOptionEnum OptionType { get { return TileMapOptionEnum.SpawnerManager; } }


    private void SetSpawner(string assetName)
    {
        foreach (var groupInfo in DL.Instance.All.SpawnerInfoData.GroupInfo)
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
                    string path = $"{group}_{case_}";
                    SpawnerData caseSpawnerData = DL.Instance.All.SpawnerData[path];
                    AllSpawner.AddRange(caseSpawnerData.Spawner);
                    foreach (var spawner in caseSpawnerData.Spawner)
                        spawner.Init();
                }
            }
        }
    }

    private void CheckSpawner(Vector2Int playerPos)
    {
        if (!_isActive)
            return;

        for (int i = 0; i < AllSpawner.Count; i++)
        {
            Spawner spawner = AllSpawner[i];
            spawner.CheckVisible();
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
        if (!_isActive)
            return;

        foreach(var spawner in ActiveSpawner)
        {
            if (spawner.IsSpawn && spawner.IsIdentify && !spawner.SpawnObj.activeSelf)
                spawner.SpawnerReset();
            spawner.TrySpawn();
        }
    }

#if UNITY_EDITOR
    private HashSet<Spawner> spawnerGizmo;

    private void OnDrawGizmos()
    {
        if (spawnerGizmo == null)
            return;

        float tileSize = MM.Instance.TileSize;

        foreach(var a in spawnerGizmo)
        {
            Vector2Int tilePos = a.TilePos;
            Gizmos.color = Color.Lerp(Color.white, Color.red, a.CurrentTime / a.CoolTime);
            Gizmos.DrawCube(new(tilePos.x * tileSize + tileSize/2, tilePos.y * tileSize + tileSize/2, 0), Vector3.one * 1f);
        }
    }
#endif
}
