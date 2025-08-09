using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawner : Spawner
{
    public MonsterEnum[]        Monsters;
    public MonsterEnum          Select;
    public float                ActiveRange;

    public MonsterSpawner(SpawnerTile spawnerTile, float genericMinRange, float genericMaxRange) : base(spawnerTile, genericMinRange, genericMaxRange)
    {
        var a = DropTableUtil.GetDropItemFromTable("TableTest1");
        UIPopUpHandler.Instance.InventoryScript.AddItem(a.data, a.amount);
    }

    public MonsterSpawner()
    {

    }

    public override void Spawn()
    {
        base.Spawn();
        // SpawnManager에서 Enum을 통해 오브젝트를 풀링후 스폰한다.
        Debug.Log($"Monster {Select}이 스폰됨!");
    }

    public override GameObject GetSpawnObject()
    {
        return SpawnerPool.Instance.MonsterPool.Get(Select);
    }

    public override void Init()
    {
        base.Init();
        if (Monsters.Length == 0)
        {
            Debug.LogWarning($"{TilePos.ToString()}에 위치한 Spawner의 Monsters가 비어있습니다.");
            Select = MonsterEnum.None;
        }
        else
        {
            Select = Monsters[Random.Range(0, Monsters.Length)];
        }
    }
}
