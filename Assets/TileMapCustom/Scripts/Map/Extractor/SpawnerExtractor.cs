using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


    private Dictionary<SpawnerGroupEnum, Dictionary<SpawnerCaseEnum, List<Spawner>>>
                            _spawners;            


    public void Extract(MapEnum mapType, ref TileMapData mapData)
    {
        mapData.All.SpawnerData = new();
        ExtractTilemapToSpawner();
        SaveSpawnerData(ref mapData);
    }

    private void ExtractTilemapToSpawner()
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
                    monster.ActiveRange = monsterTile.ActiveRange;

                    if (!_spawners.ContainsKey(spawnerTile.Group))
                        _spawners[spawnerTile.Group] = new();
                    if (!_spawners[spawnerTile.Group].ContainsKey(spawnerTile.Case))
                        _spawners[spawnerTile.Group][spawnerTile.Case] = new();

                    _spawners[spawnerTile.Group][spawnerTile.Case].Add(monster);
                    break;
                case SpawnerType.Plant:
                    PlantSpawner plant = new(spawnerTile, GenericMinRange, GenericMaxRange);
                    PlantSpawnerTile plantTile = (PlantSpawnerTile)spawnerTile;
                    plant.Plants = plantTile.Plants;

                    if (!_spawners.ContainsKey(spawnerTile.Group))
                        _spawners[spawnerTile.Group] = new();
                    if (!_spawners[spawnerTile.Group].ContainsKey(spawnerTile.Case))
                        _spawners[spawnerTile.Group][spawnerTile.Case] = new();

                    _spawners[spawnerTile.Group][spawnerTile.Case].Add(plant);
                    break;
                case SpawnerType.Mineral:
                    MineralSpawner mineral = new(spawnerTile, GenericMinRange, GenericMaxRange);
                    MineralSpawnerTile mineralTile = (MineralSpawnerTile)spawnerTile;
                    mineral.Minerals = mineralTile.Minerals;

                    if (!_spawners.ContainsKey(spawnerTile.Group))
                        _spawners[spawnerTile.Group] = new();
                    if (!_spawners[spawnerTile.Group].ContainsKey(spawnerTile.Case))
                        _spawners[spawnerTile.Group][spawnerTile.Case] = new();

                    _spawners[spawnerTile.Group][spawnerTile.Case].Add(mineral);
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

                SpawnerData spawnerData = new();
                spawnerData.Spawner = caseSpawner.Value.ToArray();

                mapData.All.SpawnerData[$"{group}_{case_}"] = spawnerData;
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
