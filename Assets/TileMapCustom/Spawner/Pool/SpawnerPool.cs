using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class SpawnerPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public int                      PoolSize;
    [Header("ResourceNode Prefab")]
    public GameObject               ResourceNodePrefab; 

    public static SpawnerPool       Instance { get { return _instance; } }
    public static SpawnerPool       _instance;

    public static string            DataFilePath = "Spawner/Prefabs/";

    public MonsterPool                     MonsterPool;
    public ResourceNodePool                ResourceNodePool;

    private Transform _monsterRoot;
    private Transform _resourceNodeRoot;

    void Awake()
    {
        if (Instance == null) _instance = this;

        _monsterRoot = new GameObject("MonsterRoot").transform;
        _resourceNodeRoot = new GameObject("ResourceNodeRoot").transform;

        _monsterRoot.SetParent(transform, true);
        _resourceNodeRoot.SetParent(transform, true);

        MonsterPool = new(DataFilePath, "Monster", _monsterRoot);
        ResourceNodePool = new();
        ResourceNodePool.Init(_resourceNodeRoot, ResourceNodePrefab);
    }
}

public class ResourceNodePool
{
    private int _poolSize;
    private Queue<ResourceNodeBase> _pool;
    private Transform _root;
    private GameObject _resourceNodePrefab;

    public void Init(Transform root, GameObject resoutceNodePrefab)
    {
        _root = root;
        _resourceNodePrefab = resoutceNodePrefab;

        AsyncOperationHandle<SpriteAtlas> handle = Addressables.LoadAssetAsync<SpriteAtlas>("ResourceNodeAtlas");
        ResourceNodeBase.SpriteAtlas = handle.WaitForCompletion();

        _pool = new();
        for (int i = 0; i < _poolSize; i++)
        {
            Generate();
        }
    }

    public ResourceNodeBase Generate()
    {
        GameObject obj = GameObject.Instantiate(_resourceNodePrefab, _root);
        obj.name = "ResourceNode";
        obj.SetActive(false);

        ResourceNodeBase resourceNode;
        resourceNode = obj.GetComponent<ResourceNodeBase>();
        resourceNode.Init();

        _pool.Enqueue(resourceNode);

        return resourceNode;
    }

    public ResourceNodeBase Get(ResourceNode_Info_ResourceNode info)
    {
        while (_pool.Count <= 0)
        {
            Generate();
        }

        ResourceNodeBase obj = _pool.Dequeue();
        obj.Set(info);
        return obj;
    }

    public void Return(ResourceNodeBase obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_root);
        _pool.Enqueue(obj);
    }
}


public class MonsterPool
{
    private readonly string _folderName;
    private readonly Transform _root;
    private readonly string _dataFilePath;
    private readonly Dictionary<string, GameObject> _caching = new();
    private readonly Dictionary<string, Queue<GameObject>> _pool = new();

    public MonsterPool(string dataFilePath, string folderName, Transform root)
    {
        _dataFilePath = dataFilePath;
        _folderName = folderName;
        _root = root;
    }

    public void Generate(string type)
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

    public GameObject Get(string type)
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

    public void Release(string type, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.position = Vector3.zero;
        obj.transform.SetParent(_root);
        _pool[type].Enqueue(obj);
    }
}
