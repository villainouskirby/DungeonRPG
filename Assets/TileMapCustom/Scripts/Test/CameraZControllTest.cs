using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZControllTest : MonoBehaviour
{
    Transform camera;

    private void Start()
    {
        camera = Camera.main.transform;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.U))
        {
            if(camera.transform.position.z < -1)
                camera.transform.position = camera.transform.position + new Vector3(0, 0, 100f * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Y))
            camera.transform.position = camera.transform.position + new Vector3(0, 0, -10f * Time.deltaTime);
    }
}
