using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private RectTransform _rect;

    private void Awake()
    {
        UnityEngine.Cursor.visible = false;
        _rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        UnityEngine.Cursor.visible = false;
        _rect.position = Input.mousePosition;
    }
}
