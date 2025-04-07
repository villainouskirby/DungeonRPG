using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private ShopUI _shopUI;

    [SerializeField] private ItemListSO<ShopItem> _shopItemList;
    [SerializeField] private ItemListSO<Item> _inventoryItemList;

    private List<ShopItem> _shopItems => _shopItemList.Items;
    private List<ShopItem> _inventorySellItems = new List<ShopItem>();
    private List<Item> _inventoryItems => _inventoryItemList.Items;

    private void Awake()
    {
        InitShop();
        InitInvenToShop();
    }

    /// <summary> 상점 열기 </summary>
    public void OpenShop() => gameObject.SetActive(true);

    /// <summary> 상점 닫기 </summary>
    public void CloseShop() => gameObject.SetActive(false);

    /// <summary>
    /// 상점 품목 초기화
    /// <para/> 한번만 초기화 해주면 됨 => 건들필요 X
    /// </summary>
    public void InitShop()
    {
        for (int i = 0; i < _shopItems.Count; i++)
        {
            _shopUI.SetShopItemSlot(i, _shopItems[i]);
        }
    }

    /// <summary>
    /// 상점에 나타나는 인벤토리 초기화
    /// <para/> 외부에서 인벤에 변화가 있을 때 마다 호출해 줘야함
    /// </summary>
    public void InitInvenToShop()
    {
        _inventorySellItems.Clear();
        
        foreach (Item item in _inventoryItems)
        {
            if (item is CountableItem ci)
            {
                AddItem(item.Data, ci.Amount);
            }
            else
            {
                AddItem(item.Data, 1);
            }
        }
    }

    public void AddItem(ItemData itemData, int amount)
    {
        int index;
        int price = 10; // DB에서 아이템 가격 불러오기

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
                        CountableItem ci = _inventoryItems[index] as CountableItem;
                        amount = _inventorySellItems[index].AddAmountAndGetExcess(amount);

                        UpdateSlot(index);
                    }
                }
                else
                {
                    // 새 아이템 생성
                    ShopItem si = new ShopItem(ciData, price, amount);

                    // 슬롯에 추가
                    index = _inventorySellItems.Count;
                    _inventorySellItems.Add(si);

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
                index = _inventorySellItems.Count;
                _inventorySellItems.Add(new ShopItem(itemData, price, 1));

                UpdateSlot(index);
            }
        }
    }

    public void UpdateSlot(int index)
    {
        ShopItem si = _inventorySellItems[index];

        _shopUI.SetInventoryItemSlot(index, si);

        if (si.Amount == 0)
        {
            _inventorySellItems.RemoveAt(index);
            _shopUI.RemoveSlot(index);
        }
        else
        {
            if (si.Data is CountableItemData)
            {
                _shopUI.SetItemAmountText(index, si.Amount);
            }
            else
            {
                _shopUI.SetItemAmountText(index);
            }
        }

    }

    private int FindCountableItemSlotIndex(CountableItemData ciData, int index)
    {
        while (++index < _inventorySellItems.Count)
        {
            if (_inventorySellItems[index].Data.ID == ciData.ID) return index;
        }
        return -1;
    }

    public void Trade(int index, int amount) 
    {
        ShopItem item = GetItemData(index);
        if (_shopUI.Type == ShopType.buy)
        {
            _inventory.AddItem(item.Data, amount);
            AddItem(item.Data, amount);
        }
        else
        {
            _inventory.RemoveItem(index, amount);
            item.SetAmount(item.Amount - amount);
            UpdateSlot(index);
        }

        _inventory.UpdateGoldAmount(item.Price * amount * (int)_shopUI.Type);
    }

    public ShopItem GetItemData(int index)
    {
        if (_shopUI.Type == ShopType.buy)
        {
            return (_shopItems.Count > index && index >= 0) ? _shopItems[index] : null;
        }
        else
        {
            return (_inventorySellItems.Count > index && index >= 0) ? _inventorySellItems[index] : null;
        }
    }

    public int GetCurrentGold() => _inventory.GetCurrentGold();
}
