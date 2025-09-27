using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerSmooth : MonoBehaviour
{
    public static FollowPlayerSmooth Instance { get { return _instance; } }
    private static FollowPlayerSmooth _instance;

    private Transform _target; // 따라갈 대상 (플레이어)

    public List<CameraAreaData> CameraAreaData;

    public float OriZ = -10f;
    public float FollowSpeed = 5f; // 따라가는 속도

    private Vector3 _velocity = Vector3.zero; // 속도 값 (SmoothDamp용)

    private void Awake()
    {
        _target = GameObject.FindWithTag("Player").transform;
        _instance = this;
        OriZ= transform.position.z;
        CameraAreaData = new();
    }

    void Update()
    {
        Vector3 targetPosition;

        if (_target == null) return;

        if (CameraAreaData.Count == 0)
        {
            targetPosition = new(_target.position.x, _target.position.y, OriZ);
        }
        else if (CameraAreaData[0].IsFixed)
        {
            targetPosition = CameraAreaData[0].FixPos;
        }
        else if (CameraAreaData[0].IsCustomZ)
        {
            targetPosition = new(_target.position.x, _target.position.y, CameraAreaData[0].CustomZ);
        }
        else
        {
            targetPosition = new(_target.position.x, _target.position.y, OriZ);
        }

        Vector3 targetPos = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, FollowSpeed * Time.deltaTime);
        transform.position = targetPos;
    }
}
