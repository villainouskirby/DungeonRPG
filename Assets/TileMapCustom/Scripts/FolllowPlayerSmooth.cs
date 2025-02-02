using UnityEngine;

public class FollowPlayerSmooth : MonoBehaviour
{
    public Transform target; // 따라갈 대상 (플레이어)
    public float followSpeed = 5f; // 따라가는 속도
    private Vector3 velocity = Vector3.zero; // 속도 값 (SmoothDamp용)

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position;
        Vector3 targetPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSpeed * Time.deltaTime);
        targetPos.z = transform.position.z;
        transform.position = targetPos;
    }
}
