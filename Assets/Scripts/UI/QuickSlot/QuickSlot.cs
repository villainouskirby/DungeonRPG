using System.Collections.Generic;
using UnityEngine;

public class QuickSlot : UIBase
{
    [SerializeField] private QuickSlotUI _quickSlotUI;
    public int SlotCount => _slotCount;
    private int _slotCount = 5;

    private Item[] _quickSlot = new Item[0];

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _quickSlotUI.InitQuickSlotUI();
    }

    public void InitQuickSlot()
    {
        var data = UIPopUpHandler.Instance.GetScript<Equipment>().GetItemData(Equipment.EquipmentType.Backpack);
        
        if (data == null)
        {
            _slotCount = 0;
        }
        else
        {
            _slotCount = (data as BackpackItemData).BackpackInfo.pouch_count;
        }


        if (_quickSlot.Length != _slotCount)
        {
            var newSlot = new Item[_slotCount];

            int idx = 0;

            for (int i = 0; i < _quickSlot.Length; i++)
            {
                if (_quickSlot[i] != null)
                {
                    if (i < _slotCount)
                    {
                        newSlot[idx++] = _quickSlot[i];
                    }
                    else
                    {
                        UnRegisterSlot(i);
                    }
                }
            }

            _quickSlot = newSlot;
            _quickSlotUI.InitLockPanels(SlotCount);
        }
    }

    public Item GetItem(int index)
    {
        return (index >= 0 && index < _quickSlot.Length) ? _quickSlot[index] : null;
    }

    private void OnEnable()
    {
        /*
        for (int i = 0; i < _quickSlot.Length; i++)
        {
            _quickSlotUI.SetSlotImage(i);
        }*/
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        gameObject.SetActive(false);
    }

    public bool AddToSlot(Item item)
    {
        for (int i = 0; i < _quickSlot.Length; i++)
        {
            if (_quickSlot[i] == null)
            {
                _quickSlot[i] = item;
                _quickSlotUI.SetSlotImage(i);
                UIPopUpHandler.Instance.GetScript<QuickSlotInGameUI>().SetQuickSlotUI();

                return true;
            }
        }

        // 슬롯 가득 참
        return false;
    }

    public void UnRegisterSlot(int index)
    {
        if (GetItem(index) == null) return;

        UIPopUpHandler.Instance.GetScript<Inventory>().AddItemForce(_quickSlot[index].Data);
        _quickSlotUI.RemoveSlot(index);
    }

    public void RemoveSlot(int index)
    {
        _quickSlot[index] = null;
    }

    public Sprite GetItemSprite(int index)
    {
        return (_quickSlot.Length > index && index >= 0 && _quickSlot[index] != null) ? _quickSlot[index].Data.IconSprite : null;
    }

    public void SwapSlots(int idx1, int idx2)
    {
        Item tempItem = _quickSlot[idx1];
        _quickSlot[idx1] = _quickSlot[idx2];
        _quickSlot[idx2] = tempItem;

        _quickSlotUI.SetSlotImage(idx1);
        _quickSlotUI.SetSlotImage(idx2);
    }
}
