using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemPool : MonoBehaviour
{
    public static DropItemPool Instance { get { return _instance; } }
    private static DropItemPool _instance;

    [Header("DropItem Prefab")]
    public GameObject DropItemPrefab;
    public int PoolSize = 5;

    private Queue<DropItem> _pool;

    public void Awake()
    {
        _instance = this;
        _pool = new();
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            Generate();
        }
    }

    public DropItem Generate()
    {
        GameObject obj = Instantiate(DropItemPrefab, transform);
        obj.name = "DropItem";
        obj.SetActive(false);

        DropItem dropItem;
        dropItem = obj.GetComponent<DropItem>();

        _pool.Enqueue(dropItem);

        return dropItem;
    }

    public DropItem Get(ItemData itemData)
    {
        while (_pool.Count <= 0)
        {
            Generate();
        }

        DropItem obj = _pool.Dequeue();
        obj.Set(itemData);
        return obj;
    }

    public void Return(DropItem obj)
    {
        obj.ResetItem();
        _pool.Enqueue(obj);
    }
}
