using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceNodeSpawner : Spawner
{
    public ResourceNodeEnum[]        ResourceNodes;
    public ResourceNodeEnum          Select;

    public ResourceNodeSpawner(SpawnerTile spawnerTile, float genericMinRange, float genericMaxRange) : base(spawnerTile, genericMinRange, genericMaxRange)
    {
    }

    public ResourceNodeSpawner()
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
        // JJJJ 스포너 로직 수정해야함 Info 자체를 넘겨줘야지 데이터를 세팅해줌으로
        // Info를 얻는 코드를 제작해야함
        //return SpawnerPool.Instance.ResourceNodePool.Get().gameObject;
        return null;
    }

    public override void Init()
    {
        base.Init();
        if (ResourceNodes.Length == 0)
        {
            Debug.LogWarning($"{TilePos.ToString()}에 위치한 Spawner의 Minerals가 비어있습니다.");
            Select = ResourceNodeEnum.None;
        }
        else
        {
            Select = ResourceNodes[Random.Range(0, ResourceNodes.Length)];
        }
    }
}
