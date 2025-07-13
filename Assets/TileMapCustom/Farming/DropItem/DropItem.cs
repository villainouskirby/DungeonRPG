using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(CircleCollider2D))]
public class DropItem : MonoBehaviour
{
    public TMP_Text Text;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private ItemData _itemData;
    private int _amount;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Set(ItemData itemData, Sprite sprite, int amount)
    {
        _itemData = itemData;
        _spriteRenderer.sprite = sprite;
        _amount = amount;

        switch (_amount)
        {
            case 0:
                Text.text = "";
                break;
            case 1:
                Text.text = "";
                break;
            default:
                Text.text = _amount.ToString();
                break;
        }
    }

    public void Get()
    {
        UIPopUpHandler.Instance.InventoryScript.AddItem(_itemData, _amount);
        DropItemPool.Instance.Return(this);
    }

    public void ResetItem()
    {
        _itemData = null;
        _spriteRenderer.sprite = null;
        _amount = 0;
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
