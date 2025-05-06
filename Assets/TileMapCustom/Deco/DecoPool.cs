using System.Collections.Generic;
using UnityEngine;

public class DecoPool : MonoBehaviour
{
    public GameObject DecoPrefab_None;
    public GameObject DecoPrefab_Circle;
    public GameObject DecoPrefab_Box;
    public GameObject DecoPrefab_Poly;
    public int PoolSize;

    private Dictionary<ColliderType, Queue<GameObject>> _poolDic;
    private Dictionary<ColliderType, GameObject> _prefabDic;

    public void Init()
    {
        _poolDic = new Dictionary<ColliderType, Queue<GameObject>>()
        {
            { ColliderType.None,  new Queue<GameObject>() },
            { ColliderType.Circle,  new Queue<GameObject>() },
            { ColliderType.Box,     new Queue<GameObject>() },
            { ColliderType.Poly, new Queue<GameObject>() }
        };

        _prefabDic = new Dictionary<ColliderType, GameObject>()
        {
            { ColliderType.Circle,  DecoPrefab_None },
            { ColliderType.Circle,  DecoPrefab_Circle },
            { ColliderType.Box,     DecoPrefab_Box },
            { ColliderType.Poly, DecoPrefab_Poly }
        };

        for(int i = 0; i < PoolSize; i++)
        {
            Generate(ColliderType.None);
            Generate(ColliderType.Circle);
            Generate(ColliderType.Box);
            Generate(ColliderType.Poly);
        }
    }

    public GameObject Generate(ColliderType type)
    {
        Queue<GameObject> pool = _poolDic[type];
        GameObject obj = Instantiate(_prefabDic[type], transform);
        obj.name = $"{type.ToString()}_Deco";
        pool.Enqueue(obj);

        return obj;
    }

    public GameObject Get(ColliderType type)
    {
        Queue<GameObject> pool = _poolDic[type];
        GameObject obj;

        while (pool.Count <= 0)
        {
            Generate(type);
            obj = pool.Dequeue();
            obj.SetActive(true);
        }

        obj = pool.Dequeue();

        return obj;
    }

    public void Return(GameObject obj, ColliderType type)
    {
        obj.SetActive(false);
        _poolDic[type].Enqueue(obj);
    }
}
