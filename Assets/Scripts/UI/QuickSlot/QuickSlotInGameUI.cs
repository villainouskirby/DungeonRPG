using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotInGameUI : MonoBehaviour
{
    [SerializeField] private QuickSlotUI _quickSlotUI;
    [SerializeField] private Image _slotImage;

    [SerializeField] private ItemsSO _quickSlotList;

    private List<Item> _items => _quickSlotList.Items;
    private List<int> _itemIndexList = new List<int>() { 0 };
    private int _slotCount => _itemIndexList.Count;

    private int _currentIndex = 0;
    private int _actualIndex => _itemIndexList.Count != 0 ? _itemIndexList[_currentIndex] : 0;

    private void Start()
    {
        SetQuickSlotUI();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            _currentIndex = (_currentIndex - 1 + _slotCount) % _slotCount;
            SetSlotSprite(_actualIndex);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (((IUsableItem)_items[_actualIndex]).Use())
            {
                _quickSlotUI.RemoveSlot(_actualIndex);
                _currentIndex = (_currentIndex + 1) % _slotCount;
                SetSlotSprite(_actualIndex);
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            _currentIndex = (_currentIndex + 1) % _slotCount;
            SetSlotSprite(_actualIndex);
        }
    }

    public void SetQuickSlotUI()
    {
        int currentIndex = _actualIndex;

        _itemIndexList.Clear();
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
                _itemIndexList.Add(i);
        }

        _currentIndex = Mathf.Max(_itemIndexList.IndexOf(currentIndex), 0);
        SetSlotSprite(_actualIndex);
    }

    private void SetSlotSprite(int index)
    {
        _slotImage.sprite = _quickSlotUI.GetItemSprite(index);
    }
}
