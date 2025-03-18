using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public int                      PoolSize;

    public static SpawnerPool       Instance { get { return _instance; } }
    public static SpawnerPool       _instance;

    public static string            DataFilePath = "Spawner/Prefabs/";

    public GenericPool<MonsterEnum>        MonsterPool;
    public GenericPool<PlantEnum>          PlantPool;
    public GenericPool<MineralEnum>        MineralPool;

    private Transform _monsterRoot;
    private Transform _plantRoot;
    private Transform _mineralRoot;

    void Awake()
    {
        if (Instance == null) _instance = this;

        _monsterRoot = new GameObject("MonsterRoot").transform;
        _mineralRoot = new GameObject("MineralRoot").transform;
        _plantRoot = new GameObject("PlantRoot").transform;

        MonsterPool = new(DataFilePath, "Monster", _monsterRoot);
        PlantPool = new(DataFilePath, "Plant", _plantRoot);
        MineralPool = new(DataFilePath, "Mineral", _mineralRoot);
    }
}


public class GenericPool<TEnum> where TEnum : Enum
{
    private readonly string _folderName;
    private readonly Transform _root;
    private readonly string _dataFilePath;
    private readonly Dictionary<TEnum, GameObject> _caching = new();
    private readonly Dictionary<TEnum, Queue<GameObject>> _pool = new();

    public GenericPool(string dataFilePath, string folderName, Transform root)
    {
        _dataFilePath = dataFilePath;
        _folderName = folderName;
        _root = root;
    }

    public void Generate(TEnum type)
    {
        if (!_caching.TryGetValue(type, out GameObject target))
        {
            string path = $"{_dataFilePath}{_folderName}/{type.ToString()}";
            target = Resources.Load<GameObject>(path);
            if (target == null)
            {
                Debug.LogError($"{type.ToString()}의 Prfabs가 존재하지 않습니다. 에러 이미지로 대체합니다.");
                target = Resources.Load<GameObject>($"{SpawnerPool.DataFilePath}Error");
            }
            _caching[type] = target;
        }

        GameObject obj = GameObject.Instantiate(target);
        obj.transform.parent = _root;
        obj.SetActive(false);

        if (!_pool.ContainsKey(type))
            _pool[type] = new();
        _pool[type].Enqueue(obj);
    }

    public GameObject Get(TEnum type)
    {
        if (!_pool.ContainsKey(type))
        {
            for(int i = 0; i < 5; i++)
                Generate(type);
        }
        if (_pool[type].Count <= 0)
            Generate(type);
        GameObject obj = _pool[type].Dequeue();
        // 로직상 스폰될때 바로 활성화 되는건 아니기에 활성화는 안시킨다.
        //obj.SetActive(true);
        return obj;
    }

    public void Release(TEnum type, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.position = Vector3.zero;
        obj.transform.SetParent(_root);
        _pool[type].Enqueue(obj);
    }
}
