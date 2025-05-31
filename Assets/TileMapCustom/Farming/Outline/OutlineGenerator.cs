using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineGenerator : MonoBehaviour
{
    [Header("Outline Setting")]
    public Color OutlineColor;
    public float PixelSize = 15; // 하드 코딩이긴한데 쩔수... 스프라이트 크기에 맞추기
    public float Threshold = 0.9f;

    private MaterialPropertyBlock _mpBlock;
    private SpriteRenderer _sr;
    private SpriteRenderer _parentSr;


    void Awake()
    {
        _mpBlock = new();

        _sr = GetComponent<SpriteRenderer>();
        _parentSr = transform.parent.GetComponent<SpriteRenderer>();
        _sr.sprite = _parentSr.sprite;

        _sr.GetPropertyBlock(_mpBlock);

        _mpBlock.SetFloat("_PixelSize", PixelSize);
        _mpBlock.SetFloat("_Threshold", Threshold);
        _mpBlock.SetColor("_OutlineColor", OutlineColor);
        _mpBlock.SetFloat("_Active", 0f);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void OnOutline()
    {
        _mpBlock.SetFloat("_Active", 1f);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void OnOutline(Color color)
    {
        _mpBlock.SetFloat("_Active", 1f);
        _mpBlock.SetColor("_OutlineColor", color);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void OffOutline()
    {
        _mpBlock.SetFloat("_Active", 0f);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void SetOutlineColor(Color color)
    {
        _mpBlock.SetColor("_OutlineColor", color);
        _sr.SetPropertyBlock( _mpBlock);
    }

    private void OnDisable()
    {
        OffOutline();
    }
}
