using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawner : Spawner
{
    public string[]        Monsters;
    public string          Select;
    public static Dictionary<string, Monster_Info_Monster> MonsterDic;

    public MonsterSpawner(SpawnerTile spawnerTile, float genericMinRange, float genericMaxRange) : base(spawnerTile, genericMinRange, genericMaxRange)
    {
    }

    public MonsterSpawner()
    {

    }

    public override void Spawn()
    {
        base.Spawn();
        Debug.Log($"Monster {Select}이 스폰됨!");
        SpawnObj.GetComponent<MonsterController>().spawner = new(TilePos.x, TilePos.y);
    }

    public override GameObject GetSpawnObject()
    {
        return SpawnerPool.Instance.MonsterPool.Get(MonsterDic[Select].Monster_id);
    }

    public override void Init()
    {
        base.Init();
        if (MonsterDic == null)
            MonsterDic = SheetDataUtil.DicByKey(Monster_Info.Monster, x => x.Monster_name);
        if (Monsters.Length == 0)
        {
            Debug.LogWarning($"{TilePos.ToString()}에 위치한 Spawner의 Monsters가 비어있습니다.");
            Select = "Error";
        }
        else
        {
            Select = Monsters[Random.Range(0, Monsters.Length)];
        }
    }
}
