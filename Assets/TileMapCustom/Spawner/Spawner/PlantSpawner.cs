using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[System.Serializable]
public class PlantSpawner : Spawner
{
    public PlantEnum[]          Plants;
    public PlantEnum            Select;

    public PlantSpawner(SpawnerTile spawnerTile, float genericMinRange, float genericMaxRange) : base(spawnerTile, genericMinRange, genericMaxRange)
    {
    }

    public override void Spawn()
    {
        base.Spawn();
        // SpawnManager에서 Enum을 통해 오브젝트를 풀링후 스폰한다.
        Debug.Log($"Plant {Select}이 스폰됨!");
    }

    public override GameObject GetSpawnObject()
    {
        return SpawnerPool.Instance.PlantPool.Get(Select);
    }

    public override void Init()
    {
        base.Init();
        if (Plants.Length == 0)
        {
            Debug.LogWarning($"{TilePos.ToString()}에 위치한 Spawner의 Plants가 비어있습니다.");
            Select = PlantEnum.None;
        }
        else
        {
            Select= Plants[Random.Range(0, Plants.Length)];
        }
    }
}
