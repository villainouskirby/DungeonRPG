using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveForwardTest : MonoBehaviour
{
    public float speed = 5.0f; // 이동 속도 설정
    public float time = 0;
    Vector2 direction;

    private void Start()
    {
        Vector2 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotZ = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
        float z = transform.rotation.eulerAngles.z;
        direction = new(Mathf.Cos(z * Mathf.Deg2Rad), Mathf.Sin(z * Mathf.Deg2Rad));
    }

    void Update()
    {

        transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;
        time += Time.deltaTime;
        if(time > 5)
            Destroy(gameObject);
    }
}
