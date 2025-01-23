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
        if (RestCapacity <= 0)
        {
            _inventoryUI.OpenExcessPopUp();
            return amount;
        }
        CalculateRestWeight(itemData.Weight, -amount);

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

        return amount;
    }

    public void UseItem(int index, int amount = 1)
    {
        RemoveItem(index, amount);
    }

    /// <summary> CountableItem을 특정 개수만큼 버리기 </summary>
    public void RemoveItem(int index, int amount)
    {
        CountableItem ci = _items[index] as CountableItem;
        ci.SetAmount(ci.Amount - amount);

        CalculateRestWeight(GetItemData(index).Weight, amount);
        UpdateSlot(index);
    }

    /// <summary> 해당 슬롯의 모든 아이템 제거 </summary>
    public void RemoveItem(int index)
    {
        CalculateRestWeight(GetItemData(index).Weight, GetItemAmount(index));
        _items.RemoveAt(index);
        UpdateWeightText();
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

    /// <summary>
    /// 현재 남은 중량 계산
    /// <para/> amount의 부호는 아이템이 줄어드는 경우 +, 아이템이 늘어나는 경우 -
    /// </summary>
    private void CalculateRestWeight(int weight, int amount = 1)
    {
        RestCapacity += weight * amount;
    }

    private void UpdateWeightText()
    {
        _inventoryUI.UpdateWeightText(_maxCapacity - RestCapacity, _maxCapacity);
    }

    public ItemData GetItemData(int index)
    {
        return _items[index].Data;
    }

    public int GetItemAmount(int index)
    {
        Item item = _items[index];
        if (item is CountableItem ci)
        {
            return ci.Amount;
        }
        else
        {
            return 1;
        }
    }
}
