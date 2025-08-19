using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : SlotInteractHandler
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private InventoryPopUpUI _inventoryPopUpUI;
    [SerializeField] private ItemGetPopUpUI _itemGetPopUpUI;

    [SerializeField] private Transform _inventoryContent;
    [SerializeField] private GameObject _itemSlotPrefab;

    [SerializeField] private TextMeshProUGUI _weightText;

    private List<ItemSlotUI> _itemSlots = new List<ItemSlotUI>();

    public void InitInventoryUI()
    {
        foreach (ItemSlotUI itemSlot in _itemSlots)
        {
            Destroy(itemSlot.gameObject);
        }
        _itemSlots.Clear();
    }

    /// <summary> 새 슬롯 추가 </summary>
    private void CreateSlot()
    {
        ItemSlotUI slotUI;
        GameObject newSlot = Instantiate(_itemSlotPrefab, _inventoryContent); // TODO => 임시로 새로 생길때마다 동적생성으로 해놨지만 나중에 Pool을 만들어 쓰는게 더 나을지도
        if ((slotUI = newSlot.GetComponent<ItemSlotUI>()) == null) slotUI = newSlot.AddComponent<ItemSlotUI>();
        _itemSlots.Add(slotUI);
    }

    /// <summary> 슬롯 기본 정보 등록 </summary>
    public void SetItemSlot(int index, ItemData data)
    {
        // 새로 추가된 경우
        if (_itemSlots.Count == index)
        {
            // 슬롯 생성
            CreateSlot();

            // 기본 정보 등록
            _itemSlots[index].SetItemInfo(data.IconSprite, data.Name, data.Weight);
        }
    }

    /// <summary> 아이템 수량 텍스트 수정 </summary>
    public void SetItemAmountText(int index, int amount = 1)
    {
        _itemSlots[index].SetItemAmount(amount);
    }

    /// <summary> 중량 텍스트 수정 </summary>
    public void UpdateWeightText(float currentCapacity, float maxCapacity)
    {
        if (_weightText == null) return;

        // 텍스트 세팅
        _weightText.text = currentCapacity.ToString() + " / " + maxCapacity.ToString();

        // 현제 중량에 따른 색 변경
        Color color;
        if (currentCapacity >= 0.9 * maxCapacity)
        {
            color = Color.red;
        }
        else if (currentCapacity >= 0.8 * maxCapacity)
        {
            color = Color.yellow;
        }
        else
        {
            color = Color.black;
        }
        _weightText.color = color;
    }

    /// <summary> 중량 초과 알림 팝업 띄우기 </summary>
    public void OpenExcessPopUp() => _inventoryPopUpUI.OpenExcessPopUP();

    /// <summary> 슬롯 제거 </summary>
    public void RemoveSlot(int index)
    {
        Destroy(_itemSlots[index].gameObject); // TODO => 위에서 말한 Instantiate와 마찬가지
        _itemSlots.RemoveAt(index);
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
        _itemGetPopUpUI.AddItemPopUpQueue(itemData, amount);
    }

    /// <returns> 해당 슬롯의 인덱스 값 </returns>
    protected int GetItemSlotIndex(ItemSlotUI slot)
    {
        return _itemSlots.IndexOf(slot);
    }

    #region Pointer Event

    public override void OnDoubleClick()
    {
        _inventory.UseItem(GetItemSlotIndex(_pointedSlot as ItemSlotUI));
    }

    public override void OnLeftClick()
    {
        
    }

    public override void OnRightClick()
    {
        _inventoryPopUpUI.OpenMenu(GetItemSlotIndex(_pointedSlot as ItemSlotUI));
        _inventoryPopUpUI.CloseInfo();
    }

    public override void OnPointerIn()
    {
        _inventoryPopUpUI.OpenInfo(GetItemSlotIndex(_pointedSlot as ItemSlotUI));
    }

    public override void OnPointerOut()
    {
        _inventoryPopUpUI.CloseInfo();
    }

    #endregion
}
