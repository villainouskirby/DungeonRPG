using System.Collections.Generic;
using UnityEngine;

public class DecoPool : MonoBehaviour
{
    [Header("Deco Prefab Settings")]
    public GameObject DecoPrefab_None;
    public GameObject DecoPrefab_Circle;
    public GameObject DecoPrefab_Box;
    public GameObject DecoPrefab_Poly;
    public int PoolSize;

    private Dictionary<ColliderType, Queue<DecoCom>> _poolDic;
    private Dictionary<ColliderType, GameObject> _prefabDic;

    public void Init()
    {
        _poolDic = new Dictionary<ColliderType, Queue<DecoCom>>()
        {
            { ColliderType.None,  new Queue<DecoCom>() },
            { ColliderType.Circle,  new Queue<DecoCom>() },
            { ColliderType.Box,     new Queue<DecoCom>() },
            { ColliderType.Poly, new Queue<DecoCom>() }
        };

        _prefabDic = new Dictionary<ColliderType, GameObject>()
        {
            { ColliderType.None,  DecoPrefab_None },
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
        Queue<DecoCom> pool = _poolDic[type];
        GameObject obj = Instantiate(_prefabDic[type], transform);
        obj.name = $"{type.ToString()}_Deco";
        obj.SetActive(false);

        DecoCom decoCom = new();
        decoCom.Sr = obj.GetComponent<SpriteRenderer>();
        
        switch(type)
        {
            case ColliderType.Box:
                decoCom.Box = obj.GetComponent<BoxCollider2D>();
                break;
            case ColliderType.Circle:
                decoCom.Circle = obj.GetComponent<CircleCollider2D>();
                break;
            case ColliderType.Poly:
                decoCom.Poly = obj.GetComponent<PolygonCollider2D>();
                break;
        }

        pool.Enqueue(decoCom);

        return obj;
    }

    public DecoCom Get(DecoObjData decoObjData)
    {
        Queue<DecoCom> pool = _poolDic[decoObjData.ColliderData.Type];
        DecoCom obj;

        while (pool.Count <= 0)
        {
            Generate(decoObjData.ColliderData.Type);
            obj = pool.Dequeue();
            obj.Sr.gameObject.SetActive(true);
        }

        obj = pool.Dequeue();

        obj.Data = decoObjData;
        obj.Sr.gameObject.name = decoObjData.Name;
        obj.Sr.gameObject.transform.localPosition = decoObjData.Position;
        obj.Sr.gameObject.transform.localRotation = decoObjData.Rotation;
        obj.Sr.gameObject.transform.localScale = decoObjData.Scale;
        obj.Sr.sprite = DecoManager.Instance.DecoAtlas.GetSprite(decoObjData.SpriteName);
        obj.Sr.color = decoObjData.Color;
        obj.Sr.sortingOrder = decoObjData.LayerIndex;
        obj.Sr.sortingLayerName = decoObjData.LayerName;

        switch (decoObjData.ColliderData.Type)
        {
            case ColliderType.Box:
                obj.Box.offset = decoObjData.ColliderData.Offset;
                obj.Box.size = decoObjData.ColliderData.Size;
                obj.Box.isTrigger = decoObjData.ColliderData.IsTrigger;
                break;
            case ColliderType.Circle:
                obj.Circle.offset = decoObjData.ColliderData.Offset;
                obj.Circle.radius = decoObjData.ColliderData.Radius;
                obj.Circle.isTrigger = decoObjData.ColliderData.IsTrigger;
                break;
            case ColliderType.Poly:
                obj.Poly.points = decoObjData.ColliderData.Points;
                obj.Poly.offset = decoObjData.ColliderData.Offset;
                obj.Poly.isTrigger = decoObjData.ColliderData.IsTrigger;
                break;
        }

        obj.Sr.gameObject.SetActive(true);

        return obj;
    }

    public void Return(DecoCom obj)
    {
        obj.Sr.gameObject.SetActive(false);
        obj.Sr.transform.SetParent(transform);
        _poolDic[obj.Data.ColliderData.Type].Enqueue(obj);
        obj.Data = null;
    }
}

public class DecoCom
{
    public DecoObjData Data;
    public SpriteRenderer Sr;
    public BoxCollider2D Box;
    public CircleCollider2D Circle;
    public PolygonCollider2D Poly;
}