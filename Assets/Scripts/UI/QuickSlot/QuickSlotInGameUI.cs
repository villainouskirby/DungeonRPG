using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotInGameUI : MonoBehaviour
{
    [SerializeField] private QuickSlotUI _quickSlotUI;
    [SerializeField] private Image _slotImage;

    private List<Item> _items = new();
    private List<int> _itemIndexList = new List<int>();
    private int _slotCount => _itemIndexList.Count;

    private int _currentIndex = 0;
    private int _actualIndex
    {
        get
        {
            if (_slotCount == 0 || _slotCount <= _currentIndex)
            {
                _currentIndex = 0;

                if (_slotCount == 0) return 0;
            }

            return _itemIndexList[_currentIndex];
        }
    }

    private void Start()
    {
        SetQuickSlotUI();
    }

    private void Update()
    {
        if (_slotCount == 0) return;

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            _currentIndex = (_currentIndex - 1 + _slotCount) % _slotCount;
            SetSlotSprite(_actualIndex);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            UseItem().Forget();
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            _currentIndex = (_currentIndex + 1) % _slotCount;
            SetSlotSprite(_actualIndex);
        }
    }

    private async UniTaskVoid UseItem()
    {
        if (await ((IUsableItem)_items[_actualIndex]).Use())
        {
            _quickSlotUI.RemoveSlot(_actualIndex);
            _itemIndexList.RemoveAt(_currentIndex);
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
