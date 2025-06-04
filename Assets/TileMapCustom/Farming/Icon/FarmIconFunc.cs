using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmIconFunc : MonoBehaviour
{
    [Header("FarmIcon Settings")]
    public Sprite ResourceNode;
    public Sprite Able;
    public Sprite Disable;

    private SpriteRenderer _baseSprite;
    private SpriteRenderer _possibleSprite;

    void Awake()
    {
        _baseSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _possibleSprite = transform.GetChild(1).GetComponent<SpriteRenderer>()  ;
    }

    public void SetIcon(bool possible)
    {
        _baseSprite.sprite = ResourceNode;
        _possibleSprite.sprite = possible ? Able : Disable;
    }
}
