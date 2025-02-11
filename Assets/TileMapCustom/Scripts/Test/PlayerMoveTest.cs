using UnityEngine;

public class PlayerMoveTest : MonoBehaviour
{
    public float speed = 5f; // 이동 속도
    private Rigidbody2D _rb;
    public GameObject TestObj;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal"); // 좌우 이동 (A, D 또는 화살표 좌우)
        float moveY = Input.GetAxis("Vertical");   // 상하 이동 (W, S 또는 화살표 위아래)

        Vector2 velocity = new Vector2(moveX, moveY).normalized * speed;
        _rb.velocity = velocity;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(TestObj, transform.position, Quaternion.identity);
        }
    }
}