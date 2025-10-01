using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class CameraArea : MonoBehaviour
{
    public CameraAreaData Data;

    private BoxCollider2D _collider;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            SetCamera();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            ResetCamera();
        }
    }

    public void Set(CameraAreaData data)
    {
        Data = data;
        _collider.size = Data.Size;
        _collider.offset = Data.Offset;
        transform.position = new(Data.Pos.x, Data.Pos.y, 0);
    }

    private void SetCamera()
    {
        FollowPlayerSmooth.Instance.CameraAreaData.Add(Data);
    }

    private void ResetCamera()
    {
        FollowPlayerSmooth.Instance.CameraAreaData.Remove(Data);
    }
}

[System.Serializable]
public class CameraAreaData
{
    [HideInInspector]
    public Vector2 Pos;
    [HideInInspector]
    public Vector2 Size;
    [HideInInspector]
    public Vector2 Offset;
    public bool IsFixed;
    [HideInInspector]
    public Vector3 FixPos;
    public bool IsCustomZ;
    public float CustomZ;
}