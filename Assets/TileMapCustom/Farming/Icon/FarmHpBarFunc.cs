using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmHpBarFunc : MonoBehaviour
{
    private SpriteRenderer _rendererBG;
    private SpriteRenderer _rendererGage;
    private MaterialPropertyBlock _block;

    private void Awake()
    {
        _rendererBG = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _rendererGage = transform.GetChild(1).GetComponent<SpriteRenderer>();
        _block = new();
    }

    public void SetAlpha(float a)
    {
        _rendererBG.color = new(_rendererBG.color.r, _rendererBG.color.g, _rendererBG.color.b, a);
        _rendererGage.color = new(_rendererGage.color.r, _rendererGage.color.g, _rendererGage.color.b, a);
    }

    public void SetGage(float fillAmount)
    {
        _block.SetFloat("_FillAmount", fillAmount);
        _rendererGage.SetPropertyBlock(_block);
    }
}
