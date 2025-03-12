using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmIconFunc : MonoBehaviour
{
    [Header("FarmIcon Settings")]
    public Sprite Mineral;
    public Sprite Plant;
    public Sprite Able;
    public Sprite Disable;

    private SpriteRenderer _baseSprite;
    private SpriteRenderer _possibleSprite;

    void Awake()
    {
        _baseSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _possibleSprite = transform.GetChild(1).GetComponent<SpriteRenderer>()  ;
    }

    public void SetIcon(FarmEnum type, bool possible)
    {
        _baseSprite.sprite = type switch
        {
            FarmEnum.Plant => Plant,
            FarmEnum.Mineral => Mineral,
            _ => throw new System.Exception("존재하지 않는 Farm Type입니다.")
        };

        _possibleSprite.sprite = possible ? Able : Disable;
    }
}
