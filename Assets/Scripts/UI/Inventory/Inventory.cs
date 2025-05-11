using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    /// <summary> 남은 가방 용량(무게) </summary>
    public int RestCapacity { get; private set; }

    [Tooltip("최대 아이템 수용 한도(무게)")]
    [SerializeField] private int _maxCapacity = 300;
    
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private Equipment _equipment;
    [SerializeField] private QuickSlot _quickSlot;
    [SerializeField] private IntVariableSO _gold;

    [SerializeField] private ItemListSO<Item> _itemList;

    protected List<Item> _items => _itemList.Items;

    private void Awake()
    {
        RestCapacity = _maxCapacity; // TODO => 상점에서 거래할때 인벤 한번 켜진게 아니면 초기화 안되서 가방에 추가 안함 => 게임 시작할 때 초기화 하도록 바꿔야 할듯
        UpdateWeightText();
        InitInventory();
    }

    /// <summary> 인벤토리 열기 </summary>
    public void OpenInventory()
    {
        gameObject.SetActive(true);
    }

    /// <summary> 인벤토리 닫기 </summary>
    public void CloseInventory() => gameObject.SetActive(false);

    public void OpenQuickSlotPanel() => _quickSlot.gameObject.SetActive(true);

    private bool IsValidIndex(int index) => index >= 0 && index < _items.Count;

    /// <summary> 인벤 초기화 </summary>
    public void InitInventory() // 창고 닫을때 인벤 초기화 하도록 호출해줘야함
    {
        _inventoryUI.InitInventoryUI();
        List<Item> tempItems = new List<Item>(_items);
        _items.Clear();
        RestCapacity = _maxCapacity;

        foreach (Item item in tempItems)
        {
            if (item is CountableItem ci)
            {
                AddItem(ci.Data, ci.Amount);
            }
            else
            {
                AddItem(item.Data);
            }
        }

        if (_quickSlot)
        {
            _quickSlot.InitQuickSlot();
        }
    }

    /// <summary> 아이템 강제로 넣기(중량제한 없이 강제로 넣음) </summary>
    public int AddItemForce(ItemData itemData, int amount = 1)
    {
        int index;

        // 장비하던 아이템이 아닐경우 중량 차지함
        if (_equipment)
        {
            if (itemData is not EquipmentItemData ||
                _equipment.GetItemData((itemData as EquipmentItemData).EquipmentType) != itemData)
            {
                CalculateRestWeight(itemData.Weight, -amount);
            }
        }
        else
        {
            CalculateRestWeight(itemData.Weight, -amount);
        }

        if (_maxCapacity > 0 && RestCapacity <= 0)
        {
            _inventoryUI.OpenExcessPopUp();
        }

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

    /// <summary> 아이템 넣기 </summary>
    public int AddItem(ItemData itemData, int amount = 1)
    {
        // 가방에 넣을 수 있는 개수 체크
        if (_maxCapacity > 0 && RestCapacity <= 0)
        {
            _inventoryUI.OpenExcessPopUp();
            return amount;
        }

        return AddItemForce(itemData, amount);
    }

    public async void UseItem(int index = -1)
    {
        if (index == -1) // -1인 경우 제일 마지막 아이템을 사용함
        {
            index = _items.Count - 1;
        }

        Item item = _items[index];
        if (item is IUsableItem uItem)
        {
            bool succeeded = await uItem.Use();

            if (succeeded)
            {
                if (item is EquipmentItem ei)
                {
                    // 인벤 속 템 제거 및 장비창에 장착
                    _equipment.Equip(ei.Data as EquipmentItemData);
                    CalculateRestWeight(item.Data.Weight, -1); // 인벤창에서는 제거 되지만 장비창에는 있기에 무게 다시 더함
                    RemoveItem(index, 1);
                }
                else
                {
                    CalculateRestWeight(item.Data.Weight);
                    Debug.Log(item.Data.Name + "사용");
                    UpdateSlot(index);
                }
            }
        }
    }

    public void SetItemToQuickSlot(int index)
    {
        Item item = _items[index];
        if (_quickSlot.AddToSlot(item.Clone()))
        {
            // 퀵슬롯에 넣기 성공
            RemoveItem(index, 1);
        }
    }

    /// <summary> index 위치의 Item을 특정 개수만큼 버리기 </summary>
    public void RemoveItem(int index, int amount)
    {
        Item item = _items[index];
        if (item is CountableItem ci)
        {
            ci.SetAmount(ci.Amount - amount);

            CalculateRestWeight(GetItemData(index).Weight, amount);
            UpdateSlot(index);
        }
        else
        {
            _inventoryUI.RemoveItem(index);
        }
    }

    /// <summary> 해당 슬롯의 모든 아이템 제거 </summary>
    public void RemoveItem(int index)
    {
        CalculateRestWeight(GetItemData(index).Weight, GetItemAmount(index));
        _items.RemoveAt(index);
        UpdateWeightText();
    }

    public int RemoveItem(ItemData itemData, int amount)
    {
        int index = 0;

        while (amount == 0 || index >= _items.Count)
        {
            Item targetItem = _items[index];
            if (targetItem.Data.ID == itemData.ID)
            {
                if (targetItem is CountableItem ci)
                {
                    int amountDiff = amount - ci.Amount;

                    if (amountDiff < 0) // 해당 슬롯에 있는 양이 더 많음
                    {
                        RemoveItem(index, -amount);
                        amount = 0;
                    }
                    else
                    {
                        if (amountDiff > 0)
                        {
                            amount = amountDiff;
                        }
                        else // amount == 0 => 슬롯에 있는 양과 소모되는 양이 같음
                        {
                            amount = 0;
                        }

                        _inventoryUI.RemoveItem(index);
                    }
                }
            }

            index++;
        }

        return amount;
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

    public int GetCurrentGold() => _gold.Value;

    public void UpdateGoldAmount(int amount)
    {
        _gold.Value += amount;
    }

    public ItemData GetItemData(int index)
    {
        return (_items.Count > index && index >= 0) ? _items[index].Data : null;
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

    public int GetItemAmountSum(long ID)
    {
        int amount = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Data.ID == ID)
            {
                amount += GetItemAmount(i);
            }
        }

        return amount;
    }

    public bool CheckItemUsable(int index) => _items[index] is IUsableItem;
}
