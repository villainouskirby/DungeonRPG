using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    /// <summary> 남은 가방 용량(무게) </summary>
    public int RestCapacity { get; private set; }

    [Tooltip("최대 아이템 수용 한도(무게)")]
    [SerializeField] private int _maxCapacity = 300;
    
    [SerializeField] private InventoryUI _inventoryUI;

    private List<Item> _items = new List<Item>();

    private void Awake()
    {
        RestCapacity = _maxCapacity;
        UpdateWeightText();
    }

    private bool IsValidIndex(int index) => index >= 0 && index < _items.Count;

    public int AddItem(ItemData itemData, int amount = 1)
    {
        int index;

        // 가방에 넣을 수 있는 개수 체크
        int restAmount = (amount * itemData.Weight > RestCapacity) ? amount - RestCapacity / itemData.Weight : 0;
        amount -= restAmount;
        RestCapacity -= amount * itemData.Weight;

        // 수량이 있는 아이템
        if (itemData is CountableItemData ciData)
        {
            // 이미 아이템이 존재하는지, 용량의 여유가 있는지 체크
            bool findNextCountable = true;
            index = -1;

            while (amount > 0)
            {
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(ciData, index);

                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    else
                    {
                        CountableItem ci = _items[index] as CountableItem;
                        amount = ci.AddAmountAndGetExcess(amount);

                        UpdateSlot(index);
                    }
                }
                else
                {
                    // 새 아이템 생성
                    CountableItem ci = ciData.Createitem() as CountableItem;
                    ci.SetAmount(amount);

                    // 슬롯에 추가
                    index = _items.Count;
                    _items.Add(ci);

                    // 남은 개수 계산
                    amount = (amount > ciData.MaxAmount) ? (amount - ciData.MaxAmount) : 0;

                    UpdateSlot(index);
                }
            }
        }
        // 수량이 없는 아이템
        else
        {
            for (; amount > 0; amount--)
            {
                // 아이템 생성 및 슬롯에 추가
                index = _items.Count;
                _items.Add(itemData.Createitem());

                UpdateSlot(index);
            }
        }

        return amount + restAmount;
    }

    private int FindCountableItemSlotIndex(CountableItemData ciData, int index)
    {
        while (++index < _items.Count)
        {
            if (_items[index].Data.ID == ciData.ID) return index;
        }
        return -1;
    }

    private void UpdateSlot(int index)
    {
        if (!IsValidIndex(index)) return;

        Item item = _items[index];

        _inventoryUI.SetItemSlot(index, item.Data);

        if (item is CountableItem ci)
        {
            if (ci.IsEmpty)
            {
                _items.RemoveAt(index);
                _inventoryUI.RemoveSlot(index);
            }
            else
            {
                _inventoryUI.SetItemAmountText(index, ci.Amount);
            }
        }
        else
        {
            _inventoryUI.SetItemAmountText(index);
        }

        UpdateWeightText();
    }

    private void UpdateWeightText()
    {
        _inventoryUI.UpdateWeightText(_maxCapacity - RestCapacity, _maxCapacity);
    }
}
