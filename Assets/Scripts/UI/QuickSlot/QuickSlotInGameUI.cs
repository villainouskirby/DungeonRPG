using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotInGameUI : MonoBehaviour
{
    [SerializeField] private QuickSlotUI _quickSlotUI;
    [SerializeField] private QuickSlot _quickSlot;
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
            Debug.Log("aaaa");
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
        if (_slotCount == 0) return;

        int idx = _actualIndex;
        Item item = _quickSlot.GetItem(idx);   // 항상 QuickSlot에서 읽기
        if (item == null)
        {
            RemoveIndexAndRefresh(_currentIndex);
            return;
        }

        if (item is not IUsableItem usable) return;

        bool ok = await usable.Use();
        if (!ok) return;

        if (item is CountableItem ci && ci.IsEmpty)
        {
            _quickSlotUI.RemoveSlot(idx);      // UI + 데이터 비우기 
            RemoveIndexAndRefresh(_currentIndex);
        }
        else
        {
            SetSlotSprite(idx);                // 유지형은 아이콘만 갱신
        }
    }
    private void RemoveIndexAndRefresh(int listIndex)
    {
        if (listIndex >= 0 && listIndex < _itemIndexList.Count)
            _itemIndexList.RemoveAt(listIndex);
        else
            RebuildIndexList();

        if (_slotCount == 0) _slotImage.sprite = null;
        else
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, _slotCount - 1);
            SetSlotSprite(_actualIndex);
        }
    }
    private void RebuildIndexList()
    {
        _itemIndexList.Clear();
        if (_quickSlot == null) return;

        for (int i = 0; i < _quickSlot.Count; i++)
            if (_quickSlot.GetItem(i) != null)
                _itemIndexList.Add(i);

        if (_currentIndex >= _slotCount) _currentIndex = Mathf.Max(0, _slotCount - 1);
    }

    public void SetQuickSlotUI()
    {
        RebuildIndexList();
        if (_slotCount == 0) _slotImage.sprite = null;
        else SetSlotSprite(_actualIndex);
    }

    private void SetSlotSprite(int index)
    {
        _slotImage.sprite = _quickSlotUI.GetItemSprite(index);
    }
}
