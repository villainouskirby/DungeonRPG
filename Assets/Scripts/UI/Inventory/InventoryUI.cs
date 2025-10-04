using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : SlotInteractHandler
{
    public enum TabType
    {
        All = 0,
        Equipment = 1,
        Usable = 2,
        Potion = 3,
        Others = 4
    }

    [Header("Scripts")]
    [SerializeField] private Inventory _inventory;
    [SerializeField] private InventoryPopUpUI _inventoryPopUpUI;
    [SerializeField] private ItemGetPopUpUI _itemGetPopUpUI;
    [SerializeField] private TabHandler _tabHandler;

    [Header("Contents")] [Tooltip("all 부터 ingredient까지 순서대로 넣어야 함")]
    [SerializeField] private Transform[] _allContents = new Transform[5];

    [Header("Prefabs")]
    [SerializeField] private GameObject _itemSlotPrefab;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _currentWeightText;
    [SerializeField] private TextMeshProUGUI _maxWeightText;

    private TabType _currentTabType;
    private Dictionary<TabType, List<InventoryItemSlotUI>> _itemSlotsDict = new();
    private Dictionary<TabType, Transform> _contentsDict = new();

    private PlayerController _playerController;

    [Header("중량 패널티 설정")]
    [Tooltip("이 비율 미만: 패널티 없음, 이 이상부터 Multiplier1 적용")]
    [Range(0f, 1f)][SerializeField] private float threshold1 = 0.8f;   // 기본 0.8
    [Range(0.1f, 1f)][SerializeField] private float multiplier1 = 0.8f; // 기본 0.8

    [Tooltip("이 비율 이상부터 Multiplier2 적용")]
    [Range(0f, 1f)][SerializeField] private float threshold2 = 0.9f;   // 기본 0.9
    [Range(0.1f, 1f)][SerializeField] private float multiplier2 = 0.6f; // 기본 0.6

    [Tooltip("threshold2 초과일 때 적용")]
    [Range(0.1f, 1f)][SerializeField] private float multiplier3 = 0.5f; // 기본 0.5

    private void OnEnable()
    {
        if (_tabHandler == null) return;

        _tabHandler.OnIndexChanged += ChangeCurrentTabType;
    }

    private void OnDisable()
    {
        if (_tabHandler == null) return;

        _tabHandler.OnIndexChanged -= ChangeCurrentTabType;
    }

    private void ChangeCurrentTabType(int index)
    {
        _currentTabType = (TabType)index;
    }

    public void AfterAwake()
    {
        for (int i = 0; i < 5; i++)
        {
            TabType type = (TabType)i;

            _itemSlotsDict[type] = new();
            _contentsDict[type] = _allContents[i];
        }
    }

    public void InitInventoryUI()
    {
        foreach (var slotList in _itemSlotsDict.Values)
        {
            foreach (var slot in slotList)
            {
                Destroy(slot.gameObject);
            }
            slotList.Clear();
        }
    }

    /// <summary> 새 슬롯 추가 </summary>
    private void CreateSlot(int index, TabType type)
    {
        InventoryItemSlotUI slotUI;
        GameObject newSlot = Instantiate(_itemSlotPrefab, _contentsDict[type]); // TODO => 임시로 새로 생길때마다 동적생성으로 해놨지만 나중에 Pool을 만들어 쓰는게 더 나을지도
        newSlot.transform.SetSiblingIndex(_inventory.GetIndexFromTypeList(index, type));

        if ((slotUI = newSlot.GetComponent<InventoryItemSlotUI>()) == null) slotUI = newSlot.AddComponent<InventoryItemSlotUI>();

        _itemSlotsDict[type].Insert(_inventory.GetIndexFromTypeList(index, type), slotUI);

        // 복제하여 전체 리스트에도 추가
        slotUI = Instantiate(slotUI, _contentsDict[TabType.All]);
        slotUI.transform.SetSiblingIndex(index);
        _itemSlotsDict[TabType.All].Insert(index, slotUI);
    }

    /// <summary> 슬롯 기본 정보 등록 </summary>
    public void RegisterItemSlot(int index, Item item, TabType type)
    {
        var data = item.Data;

        // 슬롯 생성
        CreateSlot(index, type);

        // 기본 정보 등록
        _itemSlotsDict[TabType.All][index].SetItemInfo(data.IconSprite, data.Name, data.Info.weight);
        _itemSlotsDict[TabType.All][index].SetItemDetail(data);

        int innerIndex = _inventory.GetIndexFromTypeList(index, type);
        _itemSlotsDict[type][innerIndex].SetItemInfo(data.IconSprite, data.Name, data.Info.weight);
        _itemSlotsDict[type][innerIndex].SetItemDetail(data);

        if (item is EquipmentItem ei)
        {
            ei.OnEquippedChanged += _itemSlotsDict[TabType.All][index].SetEquipped;
            ei.OnEquippedChanged += _itemSlotsDict[type][innerIndex].SetEquipped;
        }
    }

    /// <summary> 아이템 수량 텍스트 수정 </summary>
    public void SetItemAmountText(int index, TabType type, int amount = 1)
    {
        _itemSlotsDict[TabType.All][index].SetItemAmount(amount);
        _itemSlotsDict[type][_inventory.GetIndexFromTypeList(index, type)].SetItemAmount(amount);
    }

    /// <summary> 중량 텍스트 수정 </summary>
    public void UpdateWeightText(float currentCapacity, float maxCapacity)
    {
        if (_currentWeightText == null) return;

        // 텍스트 세팅
        _currentWeightText.text = currentCapacity.ToString();
        _maxWeightText.text = maxCapacity.ToString();

        // 현재 중량에 따른 색 변경
        Color color;
        if (currentCapacity >= maxCapacity)
        {
            color = Color.red;
        }
        else if (currentCapacity >= 0.9 * maxCapacity)
        {
            color = Color.yellow;
        }
        else
        {
            color = Color.white;
        }
        _currentWeightText.color = color;
        ApplyWeightPenaltyToPlayer(currentCapacity, maxCapacity);
    }
    
    private void ApplyWeightPenaltyToPlayer(float currentCapacity, float maxCapacity)
    {
        if (_playerController == null || maxCapacity <= 0f) return;

        float ratio = currentCapacity / maxCapacity;
        float m;

        // 비율별 배수 선택(인스펙터 값 사용)
        if (ratio >= threshold2)
            m = (ratio > threshold2) ? multiplier3 : multiplier2;
        else if (ratio >= threshold1)
            m = multiplier1; // 0.8 이상 ~ 0.9 미만
        else
            m = 1f; // 패널티 없음

        _playerController.SetWeightSpeedMultiplier(m);
    }

    /// <summary> 중량 초과 알림 팝업 띄우기 </summary>
    public void OpenExcessPopUp() => _inventoryPopUpUI.OpenExcessPopUP();

    /// <summary> 슬롯 제거 </summary>
    public void RemoveSlot(int index)
    {
        Destroy(_itemSlotsDict[TabType.All][index].gameObject); // TODO => 위에서 말한 Instantiate와 마찬가지
        _itemSlotsDict[TabType.All].RemoveAt(index);

        TabType itemType = _inventory.GetItemTypeByIndex(index);
        index = _inventory.GetIndexFromTypeList(index, itemType);
        Destroy(_itemSlotsDict[itemType][index].gameObject);
        _itemSlotsDict[itemType].RemoveAt(index);
    }

    public void UseItem(int index)
    {
        _inventory.UseItem(index);
    }

    public void SetItemToQuickSlot(int index)
    {
        _inventory.SetItemToQuickSlot(index);
    }

    /// <summary> CountableItem을 특정 개수만큼 버리기 </summary>
    public void RemoveItem(int index, int amount)
    {
        _inventory.RemoveItem(index, amount);
    }

    /// <summary> 해당 슬롯의 모든 아이템 제거 </summary>
    public void RemoveItem(int index)
    {
        RemoveSlot(index);
        _inventory.RemoveItem(index);
    }

    public Item GetItem(int index)
    {
        return _inventory.GetItemByIndex(index);
    }

    /// <returns> 해당 슬롯의 데이터 </returns>
    public ItemData GetItemData(int index)
    {
        return _inventory.GetItemData(index);
    }

    /// <returns> 해당 슬롯의 아이템 개수 </returns>
    public int GetItemAmount(int index)
    {
        return _inventory.GetItemAmount(index);
    }

    /// <returns> 사용가능한 아이템인지에 대한 bool값 </returns>
    public bool CheckItemUsable(int index)
    {
        return _inventory.CheckItemUsable(index);
    }

    /// <summary> 아이템 획득시 팝업될 아이템 큐에 추가 </summary>
    public void AddItemPopUpQueue(ItemData itemData, int amount = 1)
    {
        if (_itemGetPopUpUI == null) return;

        _itemGetPopUpUI.AddItemPopUpQueue(itemData, amount);
    }

    /// <returns> 해당 슬롯의 인덱스 값 </returns>
    protected int GetItemSlotIndex(InventoryItemSlotUI slot)
    {
        return _inventory.GetIndexFromAllItems(_itemSlotsDict[_currentTabType].IndexOf(slot), _currentTabType);
    }

    #region Pointer Event

    public override void OnDoubleClick()
    {
    }

    public override void OnLeftClick()
    {
        _inventoryPopUpUI.OpenMenu(GetItemSlotIndex(_pointedSlot as InventoryItemSlotUI));
    }

    public override void OnRightClick()
    {
        (_pointedSlot as InventoryItemSlotUI).ToggleDetail();
    }

    public override void OnPointerIn()
    {
    }

    public override void OnPointerOut()
    {
    }

    #endregion
}
