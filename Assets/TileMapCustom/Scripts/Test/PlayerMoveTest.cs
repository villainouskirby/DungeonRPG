using UnityEngine;

public class PlayerMoveTest : MonoBehaviour
{
    public float speed = 5f; // 이동 속도

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal"); // 좌우 이동 (A, D 또는 화살표 좌우)
        float moveY = Input.GetAxis("Vertical");   // 상하 이동 (W, S 또는 화살표 위아래)

        Vector3 move = new Vector3(moveX, moveY, 0) * speed * Time.deltaTime;
        transform.position += move;
    }
}