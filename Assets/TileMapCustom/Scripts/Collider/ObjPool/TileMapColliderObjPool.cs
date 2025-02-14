using System.Collections.Generic;
using UnityEngine;

public class TileMapColliderObjPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public int PoolSize;
    public GameObject Collider;

    public static TileMapColliderObjPool Instance { get { return _instance; } }
    public static TileMapColliderObjPool _instance;
    private Queue<GameObject> _colliderPool = new();

    void Awake()
    {
        if (Instance == null) _instance = this;

        for (int i = 0; i < PoolSize; i++)
            GenerateObj();
    }

    public void GenerateObj()
    {
        GameObject newCollider = Instantiate(Collider);
        newCollider.transform.parent = transform;
        newCollider.SetActive(false);

        _colliderPool.Enqueue(newCollider);
    }

    public GameObject GetCollider()
    {
        if (_colliderPool.Count <= 0)
            GenerateObj();

        GameObject collider = _colliderPool.Dequeue();
        collider.SetActive(true);
        return collider;
    }

    public void ReturnCollider(GameObject collider)
    {
        collider.transform.position = Vector3.zero;
        collider.transform.SetParent(transform);
        collider.SetActive(false);
        _colliderPool.Enqueue(collider);
    }
}
