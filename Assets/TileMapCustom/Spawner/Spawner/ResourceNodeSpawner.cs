using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceNodeSpawner : Spawner
{
    private static Dictionary<string, ResourceNode_Info_ResourceNode> _infoMapping;
    public string[]        ResourceNodes;
    public string          Select;

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
        Debug.Log($"Mineral {Select}이(가) 스폰됨!");
    }

    public override GameObject GetSpawnObject()
    {
        return SpawnerPool.Instance.ResourceNodePool.Get(_infoMapping[Select]).gameObject;
    }

    public override void Init()
    {
        base.Init();

        _infoMapping ??= SheetDataUtil.DicByKey(ResourceNode_Info.ResourceNode, "ResourceNode_name", _infoMapping);
        if (ResourceNodes.Length == 0)
        {
            Debug.LogWarning($"{TilePos.ToString()}에 위치한 Spawner의 ResourceNode가 비어있습니다.");
            Select = "Error";
        }
        else
        {
            Select = ResourceNodes[Random.Range(0, ResourceNodes.Length)];
        }
    }
}
