using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ContactOnCamera : MonoBehaviour
{
    public Camera TargetCamera;

    private void FixedUpdate()
    {
        Vector3 cameraPos = TargetCamera.transform.position;
        float distance = Mathf.Abs(cameraPos.z);  // 카메라의 Z값 (양수 처리)

        // Perspective 카메라의 특정 Z 거리에서 화면 크기 계산
        float height = 2.0f * distance * Mathf.Tan(TargetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * TargetCamera.aspect;
        gameObject.transform.localScale = new Vector2(width, height);
    }
}
