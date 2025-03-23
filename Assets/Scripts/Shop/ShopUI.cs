using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShopType
{
    buy = -1,
    sell = 1
}


public class ShopUI : SlotInteractHandler
{
    [SerializeField] private Shop _shop;
    [SerializeField] private ShopPopUpUI _shopPopUpUI;
    [SerializeField] private TradePopUpUI _tradePopUpUI;
    [SerializeField] private SellSelectedItemPopUpUI _sellSelectedPopUpUI;

    [SerializeField] private GameObject _itemSlotPrefab;
    [SerializeField] private GameObject _shopScrollView;
    [SerializeField] private GameObject _inventoryScrollView;
    [SerializeField] private Transform _shopContent;
    [SerializeField] private Transform _inventoryContent;

    private List<ItemSlotUI> _shopSlots = new List<ItemSlotUI>();
    private List<ItemSlotUI> _inventorySlots = new List<ItemSlotUI>();

    /// <summary> 선택된 슬롯 인덱스 </summary>
    private List<int> _selectedIndex = new List<int>();

    private ShopType _type = ShopType.buy;
    public ShopType Type => _type;

    public void CreateSlot(int index, List<ItemSlotUI> slots, ShopItem shopItem, ShopType tradeType)
    {
        ItemSlotUI slotUI;
        GameObject newSlot;
        if (tradeType == ShopType.buy)
        {
            newSlot = Instantiate(_itemSlotPrefab, _shopContent); // TODO => 임시로 새로 생길때마다 동적생성으로 해놨지만 나중에 Pool을 만들어 쓰는게 더 나을지도
        }
        else
        {
            newSlot = Instantiate(_itemSlotPrefab, _inventoryContent); // TODO => 임시로 새로 생길때마다 동적생성으로 해놨지만 나중에 Pool을 만들어 쓰는게 더 나을지도
        }
        if ((slotUI = newSlot.GetComponent<ItemSlotUI>()) == null) slotUI = newSlot.AddComponent<ItemSlotUI>();
        slots.Add(slotUI);

        // 기본정보 설정 => 상점이라 무게대신 가격으로 설정
        slots[index].SetItemInfo(shopItem.Data.IconSprite, shopItem.Data.Name, shopItem.Price);
    }

    public void RemoveSlot(int index)
    {
        Destroy(_inventorySlots[index].gameObject);
        _inventorySlots.RemoveAt(index);
    }

    public void SetShopItemSlot(int index, ShopItem shopItem)
    {
        if (_shopSlots.Count == index)
        {
            CreateSlot(index, _shopSlots, shopItem, ShopType.buy);
        }
        _shopSlots[index].SetItemAmount(shopItem.Amount);
    }

    public void SetItemAmountText(int index, int amount = 1)
    {
        _inventorySlots[index].SetItemAmount(amount);
    }

    public void SetInventoryItemSlot(int index, ShopItem shopItem)
    {
        if (_inventorySlots.Count == index)
        {
            CreateSlot(index, _inventorySlots, shopItem, ShopType.sell);
        }
    }

    public void Trade(int index, int amount)
    {
        _shop.Trade(index, amount);
    }

    public ShopItem GetItemData(int index)
    {
        return _shop.GetItemData(index);
    }

    public int GetCurrentGold() => _shop.GetCurrentGold();

    private int GetItemSlotIndex(ItemSlotUI slot)
    {
        if (Type == ShopType.buy)
        {
            return _shopSlots.IndexOf(slot);
        }
        else
        {
            return _inventorySlots.IndexOf(slot);
        }
    }

    public void ChangeToBuyWindow()
    {
        _shopScrollView.SetActive(true);
        _inventoryScrollView.SetActive(false);
        _type = ShopType.buy;
        ClearSelected();
    }

    public void ChangeToSellWindow()
    {
        _shopScrollView.SetActive(false);
        _inventoryScrollView.SetActive(true);
        _type = ShopType.sell;
    }

    public void OpenSellSelectedPopUp()
    {
        int sum = 0;
        foreach (int i in _selectedIndex)
        {
            ShopItem si = GetItemData(i);
            sum += si.Amount * si.Price;
        }
        _sellSelectedPopUpUI.SetInfo(sum);
    }

    public void SellSelectedSlot()
    {
        _selectedIndex.Sort((a, b) => b.CompareTo(a));
        foreach (int i in _selectedIndex)
        {
            _shop.Trade(i, GetItemData(i).Amount);
        }
        _selectedIndex.Clear();
    }

    public void SelectAll()
    {
        ClearSelected();

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            _selectedIndex.Add(i);
            _inventorySlots[i].GetComponent<Outline>().enabled = true;
        }
    }

    public void ClearSelected()
    {
        foreach (int i in _selectedIndex)
        {
            _inventorySlots[i].GetComponent<Outline>().enabled = false;
        }
        _selectedIndex.Clear();
    }

    public void ToggleSelect(int index)
    {
        int idx = _selectedIndex.FindIndex(e => e == index);
        if (idx == -1)
        {
            _inventorySlots[index].GetComponent<Outline>().enabled = true;
            _selectedIndex.Add(index);
        }
        else
        {
            _inventorySlots[index].GetComponent<Outline>().enabled = false;
            _selectedIndex.RemoveAt(idx);
        }
    }

    #region Pointer Event

    public override void OnDoubleClick()
    {
        _tradePopUpUI.SetItemData(GetItemSlotIndex(_pointedSlot as ItemSlotUI));
        ClearSelected();
    }

    public override void OnLeftClick()
    {
        if (_type == ShopType.sell)
            ToggleSelect(GetItemSlotIndex(_pointedSlot as ItemSlotUI));
    }

    public override void OnRightClick()
    {

    }

    public override void OnPointerIn()
    {
        _shopPopUpUI.OpenInfo(GetItemSlotIndex(_pointedSlot as ItemSlotUI));
    }

    public override void OnPointerOut()
    {
        _shopPopUpUI.CloseInfo();
    }

    #endregion
}
