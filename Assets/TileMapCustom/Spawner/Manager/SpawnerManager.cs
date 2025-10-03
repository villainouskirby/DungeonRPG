using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DL = DataLoader;
using MM = MapManager;
using TM = TileMapMaster;
using Random = UnityEngine.Random;

public class SpawnerManager : MonoBehaviour, ITileMapOption, ISave
{
    public List<MonsterSpawner> AllMonsterSpawner;
    public List<ResourceNodeSpawner> AllResourceNodeSpawner;
    public HashSet<Spawner> ActiveSpawner;

    private bool _isActive;
    private bool _isLoad;

    // Option
    public int Prime { get { return (int)TileMapOptionPrimeEnum.SpawnerManager; } }

    public void Init()
    {
        AllMonsterSpawner = new(1000);
        AllResourceNodeSpawner = new(1000);
        ActiveSpawner = new(1000);

#if UNITY_EDITOR
        spawnerGizmo = new();
#endif
    }

    public void InitMap(MapEnum mapType)
    {
        _isLoad = false;
        ResetSpawnwer();
        AllMonsterSpawner.Clear();
        AllResourceNodeSpawner.Clear();
        ActiveSpawner.Clear();
    }

    public void StartMap(MapEnum mapType)
    {
        if (!_isLoad)
        { SetSpawner(); SetMonsterChunkNav(); }

        CheckSpawner(PlayerMoveChecker.Instance.LastTilePos);

        if (!_isLoad)
        {
            for (int i = 0; i < AllMonsterSpawner.Count; i++)
            {
                AllMonsterSpawner[i].ForceSpawn();
            }
            for (int i = 0; i < AllResourceNodeSpawner.Count; i++)
            {
                AllResourceNodeSpawner[i].ForceSpawn();
            }
        }
    }

    public void SetMonsterChunkNav()
    {
        List<Vector2Int> targetChunk = new();
        for (int i = 0; i < AllMonsterSpawner.Count; i++)
        {
            Vector2Int chunkPos = ChunkManager.Instance.GetChunkPos(AllMonsterSpawner[i].TilePos);
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int correctPos = new(chunkPos.x + x, chunkPos.y + y);
                    if (correctPos.x >= 0 && correctPos.x < DL.Instance.All.Width && correctPos.y >= 0 && correctPos.y < DL.Instance.All.Height)
                        targetChunk.Add(correctPos);
                }

        }
        NavMeshManager.Instance.SetMonsterChunkNav(targetChunk);
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
        AllMonsterSpawner.Clear();
        AllResourceNodeSpawner.Clear();
        ActiveSpawner.Clear();
        _isActive = false;
    }

    private void ResetSpawnwer()
    {
        for (int i = 0; i < AllResourceNodeSpawner.Count; i++)
        {
            if (AllResourceNodeSpawner[i].SpawnObj != null)
            {
                ResourceNodeBase targetResourceNode = AllResourceNodeSpawner[i].ResourceNode;

                SpawnerPool.Instance.ResourceNodePool.Return(targetResourceNode);
            }
        }

        for (int i = 0; i < AllMonsterSpawner.Count; i++)
        {
            if (AllMonsterSpawner[i].SpawnObj != null)
            {
                SpawnerPool.Instance.MonsterPool.Release(AllMonsterSpawner[i].Select, AllMonsterSpawner[i].SpawnObj);
            }
        }

        AllMonsterSpawner.Clear();
        AllResourceNodeSpawner.Clear();
        ActiveSpawner.Clear();
    }

    public TileMapOptionEnum OptionType { get { return TileMapOptionEnum.SpawnerManager; } }


    private void SetSpawner()
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
                    if (caseSpawnerData.Monster != null)
                    {
                        AllMonsterSpawner.AddRange(caseSpawnerData.Monster);
                        foreach (var spawner in caseSpawnerData.Monster)
                            spawner.Init();
                    }
                    if (caseSpawnerData.ResourceNode != null)
                    {
                        AllResourceNodeSpawner.AddRange(caseSpawnerData.ResourceNode);
                        foreach (var spawner in caseSpawnerData.ResourceNode)
                            spawner.Init();
                    } 
                }
            }
        }
    }

    private void CheckSpawner(Vector2Int playerPos)
    {
        if (!_isActive)
            return;

        for (int i = 0; i < AllMonsterSpawner.Count; i++)
        {
            Spawner spawner = AllMonsterSpawner[i];
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

        for (int i = 0; i < AllResourceNodeSpawner.Count; i++)
        {
            Spawner spawner = AllResourceNodeSpawner[i];
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

    public void Load(SaveData saveData)
    {
        _isLoad = true;
        //AllMonsterSpawner = saveData.MonsterSpawner;
        AllResourceNodeSpawner = saveData.ResourceNodeSpawner;

        for (int i = 0; i < AllMonsterSpawner.Count; i++)
        {
            AllMonsterSpawner[i].Load(saveData);
        }

        for (int i = 0; i < AllResourceNodeSpawner.Count; i++)
        {
            AllResourceNodeSpawner[i].Load(saveData);
        }
    }

    public void Save(SaveData saveData)
    {
        for (int i = 0; i < AllMonsterSpawner.Count; i++)
        {
            AllMonsterSpawner[i].Save(saveData);
        }

        for (int i = 0; i < AllResourceNodeSpawner.Count; i++)
        {
            AllResourceNodeSpawner[i].Save(saveData);
        }
    }

    public bool IsSave => true;
#endif
}
