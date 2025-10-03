#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using EM = ExtractorMaster;

public class SpawnerExtractor : MonoBehaviour, IExtractor
{
    public Tilemap          Tilemap;

    [Header("Spawner Settings")]
    public float            GenericMinRange;
    public float            GenericMaxRange;


    private Dictionary<SpawnerGroupEnum, Dictionary<SpawnerCaseEnum, SpawnerData>>
                            _spawners;            


    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        mapData.All.SpawnerData = new();
        ExtractTilemap2Spawner();
        SaveSpawnerData(ref mapData);
    }

    private void ExtractTilemap2Spawner()
    {
        _spawners = new();

        var childs = Tilemap.transform.GetComponentsInChildren<Transform>();
        
        foreach(var child in childs)
        {
            if (child.name == Tilemap.name)
                continue;

            SpawnerTile spawnerTile = child.GetComponent<SpawnerTile>();

            switch(spawnerTile.Type)
            {
                case SpawnerType.Monster:
                    MonsterSpawner monster = new(spawnerTile, GenericMinRange, GenericMaxRange);
                    MonsterSpawnerTile monsterTile = (MonsterSpawnerTile)spawnerTile;
                    monster.Monsters = monsterTile.Monsters;

                    if (!_spawners.ContainsKey(spawnerTile.Group))
                        _spawners[spawnerTile.Group] = new();
                    if (!_spawners[spawnerTile.Group].ContainsKey(spawnerTile.Case))
                        _spawners[spawnerTile.Group][spawnerTile.Case] = new();
                    if (_spawners[spawnerTile.Group][spawnerTile.Case].Monster == null)
                        _spawners[spawnerTile.Group][spawnerTile.Case].Monster = new();

                    _spawners[spawnerTile.Group][spawnerTile.Case].Monster.Add(monster);
                    break;
                case SpawnerType.ResourceNode:
                    ResourceNodeSpawner resourceNode = new(spawnerTile, GenericMinRange, GenericMaxRange);
                    ResourceNodeSpawnerTile resourceNodeTile = (ResourceNodeSpawnerTile)spawnerTile;
                    resourceNode.ResourceNodes = resourceNodeTile.ResourceNodes;

                    if (!_spawners.ContainsKey(spawnerTile.Group))
                        _spawners[spawnerTile.Group] = new();
                    if (!_spawners[spawnerTile.Group].ContainsKey(spawnerTile.Case))
                        _spawners[spawnerTile.Group][spawnerTile.Case] = new();
                    if (_spawners[spawnerTile.Group][spawnerTile.Case].ResourceNode == null)
                        _spawners[spawnerTile.Group][spawnerTile.Case].ResourceNode = new();

                    _spawners[spawnerTile.Group][spawnerTile.Case].ResourceNode.Add(resourceNode);
                    break;
            }
        }
    }

    private void SaveSpawnerData(ref TileMapData mapData)
    {
        List<string> spawnerInfo = new();
        StringBuilder sb = new();

        foreach (var groupSpawner in _spawners)
        {
            sb.Clear();

            string group = groupSpawner.Key.ToString();
            sb.Append(group);
            sb.Append("/");

            List<string> cases = new();

            foreach (var caseSpawner in groupSpawner.Value)
            {
                string case_ = caseSpawner.Key.ToString();

                cases.Add(case_);

                mapData.All.SpawnerData[$"{group}_{case_}"] = caseSpawner.Value;
            }

            string caseInfo = string.Join("_", cases);
            sb.Append(caseInfo);

            spawnerInfo.Add(sb.ToString());

            SpawnerInfoData spawnerInfoData = new();
            spawnerInfoData.GroupInfo = spawnerInfo.ToArray();

            mapData.All.SpawnerInfoData = spawnerInfoData;
        }
    }
}
#endif