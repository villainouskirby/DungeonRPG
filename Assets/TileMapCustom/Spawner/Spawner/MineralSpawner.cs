using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MineralSpawner : Spawner
{
    public MineralEnum[]        Minerals;
    public MineralEnum          Select;

    public MineralSpawner(SpawnerTile spawnerTile, float genericMinRange, float genericMaxRange) : base(spawnerTile, genericMinRange, genericMaxRange)
    {
    }

    public MineralSpawner()
    {
        
    }

    public override void Spawn()
    {
        base.Spawn();
        // SpawnManager에서 Enum을 통해 오브젝트를 풀링후 스폰한다.
        Debug.Log($"Mineral {Select}이 스폰됨!");
    }

    public override GameObject GetSpawnObject()
    {
        return SpawnerPool.Instance.MineralPool.Get(Select);
    }

    public override void Init()
    {
        base.Init();
        if (Minerals.Length == 0)
        {
            Debug.LogWarning($"{TilePos.ToString()}에 위치한 Spawner의 Minerals가 비어있습니다.");
            Select = MineralEnum.None;
        }
        else
        {
            Select = Minerals[Random.Range(0, Minerals.Length)];
        }
    }
}
