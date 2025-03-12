using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmGageBarFunc : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _block;

    public float fillAmount = 1.0f; // 0 ~ 1 범위

    private void Awake()
    {
        _renderer= transform.GetChild(1).GetComponent<SpriteRenderer>();
        _block = new();
    }

    public void SetGage(float fillAmount)
    {
        _block.SetFloat("_FillAmount", fillAmount);
        _renderer.SetPropertyBlock(_block);
    }
}
