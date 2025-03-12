using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnerExtractor : MonoBehaviour
{
    static public string    DataFilePath = "Assets/Resources/";
    static public string    DataFileDirectory = "TileMapData/";
    static public string    SpawnerFileDirectory = "SpawnerData/";

    public Tilemap          Tilemap;

    [Header("Spawner Settings")]
    public float            GenericMinRange;
    public float            GenericMaxRange;

    private MapEnum         _mapType = MapEnum.Map1;
    private Dictionary<SpawnerGroupEnum, Dictionary<SpawnerCaseEnum, List<Spawner>>>
                            _spawners;            

    void Start()
    {
        ExtractTilemapToSpawner();
        SaveSpawnerData();
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

    private void SaveSpawnerData()
    {
        List<string> spawnerInfo = new();
        StringBuilder sb = new();


        string assetName = _mapType.ToString();
        string directoryPath = $"{DataFilePath}{DataFileDirectory}{assetName}/{SpawnerFileDirectory}";

        if (Directory.Exists(directoryPath))
            Directory.Delete(directoryPath, true);

        Directory.CreateDirectory(directoryPath);

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

                SpawnerData spawnerData = ScriptableObject.CreateInstance<SpawnerData>();
                spawnerData.Spawner = caseSpawner.Value.ToArray();


                string spawnerDataName = $"{assetName}_{group}_{case_}";

                // ScriptableObject를 에셋으로 저장
                AssetDatabase.DeleteAsset($"{directoryPath}{spawnerDataName}.asset");
                AssetDatabase.CreateAsset(spawnerData, $"{directoryPath}{spawnerDataName}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"SpawnerData asset created at: {directoryPath}");
            }

            string caseInfo = string.Join("_", cases);
            sb.Append(caseInfo);

            spawnerInfo.Add(sb.ToString());

            string InfoAssetName = _mapType.ToString();
            string InfoSpawnerDataName = $"{InfoAssetName}SpawnerInfo";

            SpawnerInfoData spawnerInfoData = ScriptableObject.CreateInstance<SpawnerInfoData>();
            spawnerInfoData.GroupInfo = spawnerInfo.ToArray();

            // 저장할 폴더 경로
            string infoDirectoryPath = $"{DataFilePath}{DataFileDirectory}{InfoAssetName}/";

            if (!Directory.Exists(infoDirectoryPath))
                Directory.CreateDirectory(infoDirectoryPath);

            // ScriptableObject를 에셋으로 저장
            AssetDatabase.DeleteAsset($"{infoDirectoryPath}{InfoSpawnerDataName}.asset");
            AssetDatabase.CreateAsset(spawnerInfoData, $"{infoDirectoryPath}{InfoSpawnerDataName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"SpawnerInfoData asset created at: {infoDirectoryPath}");
        }
    }
}
