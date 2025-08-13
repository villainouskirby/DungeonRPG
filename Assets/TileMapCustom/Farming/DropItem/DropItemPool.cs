using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class DropItemPool : MonoBehaviour
{
    public static DropItemPool Instance { get { return _instance; } }
    private static DropItemPool _instance;

    public static SpriteAtlas SpriteAtlas;

    [Header("DropItem Prefab")]
    public GameObject DropItemPrefab;
    public int PoolSize = 5;
    public Sprite ErrorSprite;

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

        if (SpriteAtlas == null)
        {
            AsyncOperationHandle<SpriteAtlas> handle = Addressables.LoadAssetAsync<SpriteAtlas>("ResourceAtlas");
            handle.WaitForCompletion();
            SpriteAtlas = handle.Result;
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
        //Debug.Log(itemData.Info.Item_sprite);
        Sprite sprite = SpriteAtlas.GetSprite(itemData.Info.sprite);
        if (sprite == null)
            sprite = ErrorSprite;
        obj.gameObject.SetActive(true);
        obj.SetSprite(sprite);
        if (sprite == null)
            sprite = ErrorSprite;
        return obj;
    }

    public void Return(DropItem obj)
    {
        obj.ResetItem();
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
