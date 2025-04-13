using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ContactOnCamera : MonoBehaviour
{
    public Camera TargetCamera;

    private void Start()
    {
        TargetCamera = TileMapMaster.Instance.TargetCamera;
    }

    private void FixedUpdate()
    {
        Vector3 cameraPos = TargetCamera.transform.position;
        float distance = Mathf.Abs(cameraPos.z);

        float height = 2.0f * distance * Mathf.Tan(TargetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * TargetCamera.aspect;
        gameObject.transform.localScale = new Vector2(width, height);
    }
}
