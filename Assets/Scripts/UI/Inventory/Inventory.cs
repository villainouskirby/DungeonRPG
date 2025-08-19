using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, ISave
{
    public event Action<int, int> OnInventoryChanged;

    /// <summary> 남은 가방 용량(무게) </summary>
    public float RestCapacity { get; private set; }

    [Tooltip("최대 아이템 수용 한도(무게)")]
    [SerializeField] private float _maxCapacity = 300;

    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private Equipment _equipment;
    [SerializeField] private QuickSlot _quickSlot;
    [SerializeField] private IntVariableSO _gold;

    public List<Item> InventoryItems => _items;
    protected List<Item> _items = new();

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
    public void InitInventory() // 창고 닫을때 인벤 초기화 하도록 호출해줘야함 // TODO => 창고도 OnInventoryChanged로 로직 바꿔야 할듯
    {
        _inventoryUI.InitInventoryUI();
        List<Item> tempItems = new List<Item>(_items);
        _items.Clear();
        RestCapacity = _maxCapacity;

        foreach (Item item in tempItems)
        {
            if (item is CountableItem ci)
            {
                AddItem(ci.Data, ci.Amount, false);
            }
            else
            {
                AddItem(item.Data, 1, false);
            }
        }

        if (_quickSlot)
        {
            _quickSlot.InitQuickSlot();
        }
    }

    /// <summary> 아이템 강제로 넣기(중량제한 없이 강제로 넣음) </summary>
    public int AddItemForce(ItemData itemData, int amount = 1, bool isGetItem = true)
    {
        int index;

        // 장비하던 아이템이 아닐경우 중량 차지함
        if (_equipment)
        {
            if (itemData is not EquipmentItemData ||
                isGetItem)
            {
                CalculateRestWeight(itemData.Weight, -amount);
            }
        }
        else
        {
            CalculateRestWeight(itemData.Weight, -amount);
        }

        if (_maxCapacity > 0 && RestCapacity <= 0 && isGetItem)
        {
            _inventoryUI.OpenExcessPopUp();
        }

        // 아이템 획득시 팝업되도록 순서대로 큐에 저장
        if (isGetItem)
        {
            _inventoryUI.AddItemPopUpQueue(itemData, amount);
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
                        OnInventoryChanged?.Invoke(index, ci.Amount);
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
                    OnInventoryChanged?.Invoke(index, ci.Amount);
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
                OnInventoryChanged?.Invoke(index, 1);
            }
        }

        return amount;
    }

    /// <summary> 아이템 넣기 </summary>
    public int AddItem(ItemData itemData, int amount = 1, bool isGetItem = true)
    {
        // 가방에 넣을 수 있는 개수 체크
        if (_maxCapacity > 0 && RestCapacity <= 0)
        {
            _inventoryUI.OpenExcessPopUp();
            return amount;
        }

        return AddItemForce(itemData, amount, isGetItem);
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

    /// <summary> 인벤토리 내부 정렬 </summary>
    public void SortInventory()
    {

    }

    public void SetItemToQuickSlot(int index)
    {
        Item item = _items[index];
        if (item is not IUsableItem)
        {
            Debug.LogWarning("QuickSlot: 사용 불가능한 아이템은 퀵슬롯에 넣지 않습니다.");
            return;
        }

        if (_quickSlot.AddToSlot(item.Clone()))
        {
            RemoveItem(index, 1);
        }
    }

    /// <summary> index 위치의 Item을 특정 개수만큼 버리기 </summary>
    /// <returns> 남은 제거해야할 양 </returns>
    public int RemoveItem(int index, int amount)
    {
        Item item = _items[index];
        if (item is CountableItem ci)
        {
            int restAmount = Math.Max(amount - ci.Amount, 0);

            ci.SetAmount(ci.Amount - amount);

            CalculateRestWeight(GetItemData(index).Weight, amount - restAmount);
            UpdateSlot(index);
            OnInventoryChanged?.Invoke(index, ci.Amount);

            return restAmount;
        }
        else
        {
            _inventoryUI.RemoveItem(index);

            return amount - 1;
        }
    }

    /// <summary> 해당 슬롯의 모든 아이템 제거 </summary>
    public void RemoveItem(int index)
    {
        CalculateRestWeight(GetItemData(index).Weight, GetItemAmount(index));
        _items.RemoveAt(index);
        UpdateWeightText();
        OnInventoryChanged?.Invoke(index, 0);
    }

    /// <summary> 인벤에 있는 해당 아이템 제거 </summary>
    public int RemoveItem(ItemData itemData, int amount) // 대장장이나 그런곳에서 아이템 빠질때 사용할듯
    {
        int index = _items.Count - 1;

        while (amount == 0 || index >= 0)
        {
            Item targetItem = _items[index];
            if (targetItem.Data.SID == itemData.SID)
            {
                amount = RemoveItem(index, amount);
            }

            index--;
        }

        return amount;
    }

    private int FindCountableItemSlotIndex(CountableItemData ciData, int index)
    {
        while (++index < _items.Count)
        {
            if (_items[index].Data.SID == ciData.SID) return index;
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
    private void CalculateRestWeight(float weight, int amount = 1)
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

    public int GetItemAmountSum(string SID)
    {
        int amount = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Data.SID == SID)
            {
                amount += GetItemAmount(i);
            }
        }

        return amount;
    }

    public bool CheckItemUsable(int index) => _items[index] is IUsableItem;

    public virtual void Load(SaveData saveData)
    {
        _items = saveData.Items;
    }

    public virtual void Save(SaveData saveData)
    {
        saveData.Items = _items;
    }
}
