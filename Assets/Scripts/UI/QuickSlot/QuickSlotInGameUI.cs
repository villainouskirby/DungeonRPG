using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotInGameUI : UIBase
{
    [SerializeField] private Image _slotImage;

    private QuickSlot _quickSlot;
    private QuickSlotUI _quickSlotUI;

    private List<int> _itemIndexList = new();
    private int _slotCount => _itemIndexList.Count;

    private int _currentIndex = 0;
    private int _actualIndex
    {
        get
        {
            if (_slotCount == 0 || _slotCount <= _currentIndex)
            {
                _currentIndex = 0;

                if (_slotCount == 0) return -1;
            }

            return _itemIndexList[_currentIndex];
        }
    }

    protected override void InitBase()
    {
        _isActvieOnStart = true;
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    private void Start()
    {
        _quickSlot = UIPopUpHandler.Instance.GetScript<QuickSlot>();
        _quickSlotUI = _quickSlot.GetComponent<QuickSlotUI>();

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

        int currentIdx = _currentIndex;
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
            RemoveIndexAndRefresh(currentIdx);
        }
    }

    private void RemoveIndexAndRefresh(int listIndex)
    {
        if (listIndex >= 0 && listIndex < _itemIndexList.Count)
        {
            _itemIndexList.RemoveAt(listIndex);
        }

        if (listIndex < _currentIndex)
        {
            _currentIndex = listIndex;
        }

        RebuildIndexList();
        SetSlotSprite(_actualIndex);
    }

    private void RebuildIndexList()
    {
        _itemIndexList.Clear();

        if (_quickSlot == null) return;

        for (int i = 0; i < _quickSlot.SlotCount; i++)
        {
            if (_quickSlot.GetItem(i) != null)
            {
                _itemIndexList.Add(i);
            }
        }

        if (_currentIndex >= _slotCount) _currentIndex = 0;
    }

    public void SetQuickSlotUI()
    {
        RebuildIndexList();
        SetSlotSprite(_actualIndex);
    }

    private void SetSlotSprite(int index = -1)
    {
        if (index < 0)
        {
            ChangeAlpha(0);
        }
        else
        {
            ChangeAlpha(1);
            _slotImage.sprite = _quickSlotUI.GetItemSprite(index);
        }
    }

    private void ChangeAlpha(float a)
    {
        Color color = _slotImage.color;
        color.a = a;
        _slotImage.color = color;
    }
}
