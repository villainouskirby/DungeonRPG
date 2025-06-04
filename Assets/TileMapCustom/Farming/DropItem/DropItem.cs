using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(CircleCollider2D))]
public class DropItem : MonoBehaviour
{
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private ItemData _itemData;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Set(ItemData itemData)
    {
        _itemData = itemData;
        _spriteRenderer.sprite = itemData.IconSprite;
    }

    public void Get()
    {
        UIPopUpHandler.Instance.InventoryScript.AddItem(_itemData);
        DropItemPool.Instance.Return(this);
    }

    public void ResetItem()
    {
        _itemData = null;
        _spriteRenderer.sprite = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            PlayerFarming.Instance.TargetDropItem = this;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && PlayerFarming.Instance.TargetDropItem == this)
            PlayerFarming.Instance.TargetDropItem = null;
    }
}
