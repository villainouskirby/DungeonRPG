using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Camera TargetCamera;

    private void Start()
    {
        TargetCamera = TileMapMaster.Instance.TargetCamera;
    }

    void Update()
    {
        float z = transform.position.z;
        Vector3 cameraPos = TargetCamera.transform.position;
        gameObject.transform.position = new Vector3(cameraPos.x, cameraPos.y, z);
    }
}
