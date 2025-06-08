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
        _sr.GetPropertyBlock(_mpBlock);
        _mpBlock.SetFloat("_Active", 1f);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void OnOutline(Color color)
    {
        _sr.GetPropertyBlock(_mpBlock);
        _mpBlock.SetFloat("_Active", 1f);
        _mpBlock.SetColor("_OutlineColor", color);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void OffOutline()
    {
        _sr.GetPropertyBlock(_mpBlock);
        _mpBlock.SetFloat("_Active", 0f);
        _sr.SetPropertyBlock(_mpBlock);
    }

    public void SetOutlineColor(Color color)
    {
        _sr.GetPropertyBlock(_mpBlock);
        _mpBlock.SetColor("_OutlineColor", color);
        _sr.SetPropertyBlock( _mpBlock);
    }

    private void OnDisable()
    {
        OffOutline();
    }

    private void OnEnable()
    {
        _sr.sprite = _parentSr.sprite;
        /*
        _mpBlock.SetTexture("_MainTex", _sr.sprite.texture);

        Rect rect = _sr.sprite.textureRect;
        float texW = _sr.sprite.texture.width;
        float texH = _sr.sprite.texture.height;
        Vector4 st = new(
            rect.x / texW,
            rect.y / texH,
            rect.width / texW,
            rect.height / texH
        );
        _mpBlock.SetVector("_MainTex_ST", st);

        // TexelSize: (1/width, 1/height, width, height)
        Vector4 texelSize = new(
            1f / texW,
            1f / texH,
            texW,
            texH
        );
        _mpBlock.SetVector("_MainTex_TexelSize", texelSize);

        _sr.SetPropertyBlock(_mpBlock);
        */
    }
}
