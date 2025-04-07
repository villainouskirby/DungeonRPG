using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Camera TargetCamera;
    private Vector3 _objPos;

    private void Awake()
    {
        TargetCamera = TileMapMaster.Instance.TargetCamera;
    }

    private void Start()
    {
        _objPos = transform.position - TargetCamera.transform.position;
        _objPos.z = 0;
    }

    void FixedUpdate()
    {
        Vector3 cameraPos = TargetCamera.transform.position;
        gameObject.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0) + _objPos;
    }
}
