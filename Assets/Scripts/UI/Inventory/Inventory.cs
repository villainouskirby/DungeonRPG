using System;
using System.Collections.Generic;
using DBUtility;
using Events;
using UnityEngine;
using ItemType = InventoryUI.TabType;

public class Inventory : UIBase, ISave
{
    public event Action<int, int> OnInventoryChanged;

    /// <summary> 남은 가방 용량(무게) </summary>
    public float RestCapacity { get; private set; }

    [Tooltip("최대 아이템 수용 한도(무게)")]
    [SerializeField] private float _maxCapacity = 300;

    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private Equipment _equipment;
    [SerializeField] private IntVariableSO _gold;

    // --- Items ---

    /// <summary>
    /// 기본적으로 데이터 전달은 전체 아이템 리스트의 index를 통해 전달함
    /// <para/> 세부 항목에서의 index가 필요할 시 GetIndexFromTypeList 메서드 사용
    /// </summary>
    protected List<Item> _items = new();

    protected Dictionary<ItemType, int> _slotCountDict = new();
    protected Dictionary<ItemType, Func<int>> _indexDict = new();

    protected int GetAllItemIndex() => 0;
    protected int GetUsableItemIndex() => 0;
    protected int GetPotionItemIndex() => GetUsableItemIndex() + _slotCountDict[ItemType.Usable];
    protected int GetOthersItemIndex() => GetPotionItemIndex() + _slotCountDict[ItemType.Potion];
    protected int GetEquipmentItemIndex() => GetOthersItemIndex() + _slotCountDict[ItemType.Others];

    /// <returns> 총 아이템(슬롯)수 </returns>
    public int GetItemsCount()
    {
        return _items.Count;
    }

    /// <summary> 전체에서의 index => 세부 항목에서의 index </summary>
    public int GetIndexFromTypeList(int index)
    {
        return GetIndexFromTypeList(index, GetItemTypeByIndex(index));
    }

    /// <summary> 전체에서의 index => 세부 항목에서의 index </summary>
    public int GetIndexFromTypeList(int index, ItemType type)
    {
        return index - _indexDict[type]();
    }

    /// <summary> 세부 항목에서의 index => 전체에서의 index </summary>
    public int GetIndexFromAllItems(int index, ItemType type)
    {
        return index + _indexDict[type]();
    }

    /// <returns> 해당 인덱스에 해당하는 아이템 </returns>
    /// <param name="index"> 전체에서의 index </param>
    public Item GetItemByIndex(int index)
    {
        if (index < 0 || index >= _items.Count) return null;

        return _items[index];
    }

    /// <returns> 해당 인덱스에 해당하는 아이템 </returns>
    /// <param name="index"> 세부 항목에서의 index </param>
    public Item GetItemByIndex(int index, ItemType type)
    {
        return GetItemByIndex(GetIndexFromAllItems(index, type));
    }

    /// <returns> 해당 아이템의 타입 </returns>
    public ItemType GetItemType(Item item)
    {
        return item switch
        {
            EquipmentItem => ItemType.Equipment,
            PotionItem => ItemType.Potion,
            ResourceItem => ItemType.Others,
            IUsableItem => ItemType.Usable,
            _ => ItemType.Others,
        };
    }

    public ItemType GetItemTypeByIndex(int index)
    {
        return GetItemType(GetItemByIndex(index));
    }

    // -------------

    protected void OnEnable()
    {
        UIPopUpHandler.Instance.GetScript<Palette>().SetPalette(0);
        SoundManager.Instance.PlaySound2D("OpenBag");
    }

    protected override void Awake()
    {
        _isActvieOnStart = true;
        base.Awake();

        RestCapacity = _maxCapacity; // TODO => 상점에서 거래할때 인벤 한번 켜진게 아니면 초기화 안되서 가방에 추가 안함 => 게임 시작할 때 초기화 하도록 바꿔야 할듯

        _indexDict[ItemType.All] = GetAllItemIndex;
        _indexDict[ItemType.Usable] = GetUsableItemIndex;
        _indexDict[ItemType.Potion] = GetPotionItemIndex;
        _indexDict[ItemType.Others] = GetOthersItemIndex;
        _indexDict[ItemType.Equipment] = GetEquipmentItemIndex;

        UpdateWeightText();

        _inventoryUI.AfterAwake();

        InitInventory();
    }

    private void Start()
    {
        UIPopUpHandler.Instance.GetScript<QuickSlot>().InitQuickSlot();
        _inventoryUI.AfterStart();
        gameObject.SetActive(false);
    }

    /// <summary> 인벤토리 열기 </summary>
    public void OpenInventory()
    {
        gameObject.SetActive(true);
    }

    /// <summary> 인벤토리 닫기 </summary>
    public void CloseInventory() => gameObject.SetActive(false);

    public void OpenQuickSlotPanel() => UIPopUpHandler.Instance.OpenUI<QuickSlot>();

    private bool IsValidIndex(int index) => index >= 0 && index < _items.Count;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    /// <summary> 인벤 초기화 </summary>
    public void InitInventory() // 창고 닫을때 인벤 초기화 하도록 호출해줘야함 // TODO => 창고도 OnInventoryChanged로 로직 바꿔야 할듯 // 냅다 다 빼고 넣는거보다 슬롯 각각 업데이트 하는식으로 바꾸는것도 나쁘지 않을듯
    {
        _inventoryUI.InitInventoryUI();
        List<Item> tempItems = new List<Item>(_items);
        _items.Clear();
        RestCapacity = _maxCapacity;

        for (int i = 1; i < 5; i++)
        {
            _slotCountDict[(ItemType)i] = 0;
        }

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
    }

    /// <summary> 아이템 강제로 넣기(중량제한 없이 강제로 넣음) </summary>
    public int AddItemForce(ItemData itemData, int amount = 1, bool isGetItem = true)
    {
        using (InventoryChangedEventArgs args = InventoryChangedEventArgs.Get())
        {
            args.Init(itemData, amount);
            EventManager.Instance.InventoryChangedEvent.Invoke(args);
            args.Clear();
            args.Release();
        }

        int startIndex;
        int endIndex;
        ItemType type;

        switch (itemData.Createitem())
        {
            case EquipmentItem:
                startIndex = _indexDict[ItemType.Equipment]();
                endIndex = _indexDict[ItemType.Usable]();
                type = ItemType.Equipment;
                break;

            case PotionItem:
                startIndex = _indexDict[ItemType.Potion]();
                endIndex = _indexDict[ItemType.Others]();
                type = ItemType.Potion;
                break;

            case ResourceItem:
                goto default;

            case IUsableItem:
                startIndex = _indexDict[ItemType.Usable]();
                endIndex = _indexDict[ItemType.Potion]();
                type = ItemType.Usable;
                break;

            default:
                startIndex = _indexDict[ItemType.Others]();
                endIndex = _items.Count;
                type = ItemType.Others;
                break;
        }

        startIndex--;
        bool isAddable = FindSlotIndex(itemData, ref startIndex, endIndex);

        CalculateRestWeight(itemData.Info.weight, -amount);

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
            if (ciData.MaxAmount == 0)
            {
                Debug.LogError("ItemData Setting Error!");
                return amount;
            }

            while (amount > 0)
            {
                if (isAddable)
                {
                    CountableItem ci = _items[startIndex] as CountableItem;
                    amount = ci.AddAmountAndGetExcess(amount);

                    UpdateSlot(startIndex);
                    OnInventoryChanged?.Invoke(startIndex, ci.Amount);
                }
                else
                {
                    // 새 아이템 생성
                    CountableItem ci = ciData.Createitem() as CountableItem;
                    ci.SetAmount(amount);

                    // 슬롯에 추가
                    _items.Insert(startIndex, ci);
                    _slotCountDict[type]++;

                    // 남은 개수 계산
                    amount = (amount > ciData.MaxAmount) ? (amount - ciData.MaxAmount) : 0;

                    UpdateSlot(startIndex, true);
                    OnInventoryChanged?.Invoke(startIndex, ci.Amount);
                }

                startIndex++;
            }
        }
        // 수량이 없는 아이템
        else
        {
            for (; amount > 0; amount--, startIndex++)
            {
                // 아이템 생성 및 슬롯에 추가
                _items.Insert(startIndex, itemData.Createitem());
                _slotCountDict[type]++;

                UpdateSlot(startIndex, true);
                OnInventoryChanged?.Invoke(startIndex, 1);
            }
        }

        return amount;
    }

    /// <summary> 아이템 넣기 </summary>
    public int AddItem(ItemData itemData, int amount = 1, bool isGetItem = true)
    {
        // 가방에 넣을 수 있는 개수 체크
        if (_maxCapacity > 0 && RestCapacity <= -0.2f * _maxCapacity)
        {
            _inventoryUI.OpenExcessPopUp();
            return amount;
        }

        return AddItemForce(itemData, amount, isGetItem);
    }

    /// <summary> index는 참조, 수량만 추가해도 되는 경우 true 그렇지 않으면 false </summary>
    private bool FindSlotIndex(ItemData data, ref int startIndex, int endIndex)
    {
        while (++startIndex < endIndex)
        {
            int comparer = _items[startIndex].Data.SID.CompareTo(data.SID);

            if (comparer == 0)
            {
                if (_items[startIndex] is CountableItem ci)
                {
                    if (ci.IsMax)
                    {
                        continue;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    continue;
                }
            }
            else if (comparer < 0) return false;
        }

        return false;
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
            if (item is not EquipmentItem)
            {
                gameObject.SetActive(false);
            }

            bool succeeded = await uItem.Use();

            if (succeeded)
            {
                if (item is EquipmentItem ei)
                {
                    // 해당 슬롯 UI 업데이트
                    _equipment.Equip(ei);

                    using (var args = InventoryBehaviorEventArgs.Get())
                    {
                        args.Init(InventoryBehaviorEventArgs.Behavior.Equip, ei.Data.SID);
                        EventManager.Instance.InventoryBehaviorEvent.Invoke(args);
                        args.Release();
                    }
                }
                else
                {
                    using (var args = InventoryChangedEventArgs.Get())
                    {
                        args.Init(item.Data, -1);
                        EventManager.Instance.InventoryChangedEvent.Invoke(args);
                        args.Release();
                    }
                    CalculateRestWeight(item.Data.Info.weight);
                    Debug.Log(item.Data.Name + "사용");
                    UpdateSlot(index);
                }
            }
        }
    }

    public void SetItemToQuickSlot(int index)
    {
        Item item = GetItemByIndex(index);
        if (item is not IUsableItem)
        {
            Debug.LogWarning("QuickSlot: 사용 불가능한 아이템은 퀵슬롯에 넣지 않습니다.");
            return;
        }

        if (UIPopUpHandler.Instance.GetScript<QuickSlot>().AddToSlot(item.Clone()))
        {
            using (var args = InventoryBehaviorEventArgs.Get())
            {
                args.Init(InventoryBehaviorEventArgs.Behavior.Register, item.Data.SID);
                EventManager.Instance.InventoryBehaviorEvent.Invoke(args);
                args.Release();
            }

            RemoveItem(index, 1);
        }
    }

    /// <summary> index 위치의 Item을 특정 개수만큼 버리기 </summary>
    /// <returns> 남은 제거해야할 양 </returns>
    public int RemoveItem(int index, int amount)
    {
        Item item = _items[index];

        using (var args = InventoryChangedEventArgs.Get())
        {
            args.Init(item.Data, -amount);
            EventManager.Instance.InventoryChangedEvent.Invoke(args);
            args.Release();
        }

        if (item is CountableItem ci)
        {
            int restAmount = Math.Max(amount - ci.Amount, 0);

            ci.SetAmount(ci.Amount - amount);

            CalculateRestWeight(GetItemData(index).Info.weight, amount - restAmount);
            UpdateSlot(index);

            return restAmount;
        }
        else
        {
            _slotCountDict[GetItemTypeByIndex(index)]--;
            _inventoryUI.RemoveItem(index);

            return amount - 1;
        }
    }

    /// <summary> 해당 슬롯의 모든 아이템 제거 </summary>
    public void RemoveItem(int index)
    {
        var data = GetItemData(index);
        int amount = GetItemAmount(index);

        _slotCountDict[GetItemTypeByIndex(index)]--;
        CalculateRestWeight(data.Info.weight, amount);

        var item = _items[index];
        if (item is EquipmentItem ei)
        {
            ei.OnEquippedChanged = null;
        }

        _items.RemoveAt(index);
        UpdateWeightText();

        using (var args = InventoryChangedEventArgs.Get())
        {
            args.Init(data, -amount);
            EventManager.Instance.InventoryChangedEvent.Invoke(args);
            args.Release();
        }
        OnInventoryChanged?.Invoke(index, 0);
    }

    /// <summary> 인벤에 있는 해당 아이템 제거 </summary>
    public int RemoveItem(ItemData itemData, int amount) // 대장장이나 그런곳에서 아이템 빠질때 사용할듯
    {
        int index = _items.Count - 1;

        using (var args = InventoryChangedEventArgs.Get())
        {
            args.Init(itemData, -amount);
            EventManager.Instance.InventoryChangedEvent.Invoke(args);
            args.Release();
        }

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

    private void UpdateSlot(int index, bool isNew = false)
    {
        if (!IsValidIndex(index)) return;

        Item item = _items[index];
        ItemType type = GetItemType(item);

        if (isNew)
        {
            _inventoryUI.RegisterItemSlot(index, item, type);
        }

        if (item is CountableItem ci)
        {
            if (ci.IsEmpty)
            {
                _inventoryUI.RemoveItem(index);
            }
            else
            {
                _inventoryUI.SetItemAmountText(index, type, ci.Amount);
            }
        }
        else
        {
            _inventoryUI.SetItemAmountText(index, type);
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

        if (RestCapacity < -0.2f * _maxCapacity)
        {

        }
        else if (RestCapacity < -0.1f * _maxCapacity)
        {

        }
    }

    public void ChangeMaxCapacity(float weight)
    {
        RestCapacity += weight - _maxCapacity;
        _maxCapacity = weight;
        UpdateWeightText();
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
    
    public void GiveInitEquipment()
    {
        if (_items.Count == 0)
        {
            // 기본 아이템 장착(임시)
            AddItemForce(ItemDataConstructor.GetItemData("ARM_001"));
            AddItemForce(ItemDataConstructor.GetItemData("BPK_001"));

            UseItem(0);
            UseItem(1);
        }
    }

    public InventoryItemSlotUI GetItemSlotUI(string id)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Data.SID == id)
            {
                return _inventoryUI.GetItemSlotUI(i);
            }
        }

        return null;
    }

    public ItemType GetTabType()
    {
        return _inventoryUI.CurrentTabType;
    }

    public bool CheckItemUsable(int index) => _items[index].Data.Info.usable;

    public virtual void Load(SaveData saveData)
    {
        _items = saveData.Items;
    }

    public virtual void Save(SaveData saveData)
    {
        saveData.Items = _items;
    }
}
