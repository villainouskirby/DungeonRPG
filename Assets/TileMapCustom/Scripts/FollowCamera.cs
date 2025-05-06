using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Camera TargetCamera;
    private Vector3 _objPos;

    private void Start()
    {
        float z = transform.position.z;
        TargetCamera = TileMapMaster.Instance.TargetCamera;
        _objPos = transform.position - TargetCamera.transform.position;
        _objPos.z = z;
    }

    void FixedUpdate()
    {
        float z = transform.position.z;
        Vector3 cameraPos = TargetCamera.transform.position;
        gameObject.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0) + _objPos;
    }
}
