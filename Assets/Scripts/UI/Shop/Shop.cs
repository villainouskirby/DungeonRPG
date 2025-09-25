using System.Collections.Generic;
using UnityEngine;

public class Shop : UIBase
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private ShopUI _shopUI;

    private List<Item> _shopItems = new(); // db에서 판매목록 리스트 가져와야함

    protected override void Awake()
    {
        base.Awake();

        // db에서 상점데이터 가져오기
        InitShop();
    }

    private void OnEnable()
    {
        InitInvenToShop();
        _inventory.OnInventoryChanged += UpdateSlot;
    }

    private void OnDisable()
    {
        _inventory.OnInventoryChanged -= UpdateSlot;
    }

    /// <summary> 상점 열기 </summary>
    public void OpenShop() => gameObject.SetActive(true);

    /// <summary> 상점 닫기 </summary>
    public void CloseShop() => gameObject.SetActive(false);

    public void AddShopItem(Item item)
    {
        _shopItems.Add(item);
    }

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    /// <summary>
    /// 상점 품목 초기화
    /// <para/> 변동사항 있을 시 _shotItems 변경이 완료 된 후 호출
    /// </summary>
    public void InitShop()
    {
        int i;

        for (i = 0; i < _shopItems.Count; i++)
        {
            _shopUI.SetShopItemSlot(i, _shopItems[i]);
        }

        _shopUI.RemoveGarbageSlots(i, ShopType.purchase);
    }

    /// <summary>
    /// 상점에 나타나는 인벤토리 초기화
    /// </summary>
    public void InitInvenToShop()
    {
        int i;

        for (i = 0; i < _inventory.GetItemsCount(); i++)
        {
            Item item = _inventory.GetItemByIndex(i);

            if (item is CountableItem ci)
            {
                UpdateSlot(i, ci.Amount);
            }
            else
            {
                UpdateSlot(i, 1);
            }
        }

        _shopUI.RemoveGarbageSlots(i, ShopType.sell);
    }

    public void ResetShop()
    {
        _shopItems.Clear();
        InitShop();
    }

    /// <summary> 슬롯 아이템 수 업데이트 </summary>
    /// <param name="amount"> 현재 아이템 수 </param>
    public void UpdateSlot(int index, int amount)
    {
        if (amount <= 0)
        {
            _shopUI.RemoveSlot(index);
            return;
        }

        Item item = _inventory.GetItemByIndex(index);

        _shopUI.SetInventoryItemSlot(index, item);

        if (item is CountableItem)
        {
            _shopUI.SetItemAmountText(index, amount);
        }
        else
        {
            _shopUI.SetItemAmountText(index);
        }
    }

    public void Trade(int index, int amount) 
    {
        Item item = GetItemData(index);
        int price;

        switch (_shopUI.Type)
        {
            case ShopType.purchase:
                _inventory.AddItem(item.Data, amount);
                //price = item.Data.Info.purchase_price;
                break;

            case ShopType.sell:
                _inventory.RemoveItem(index, amount);
                price = item.Data.Info.sell_price;
                break;

            default:
                Debug.Log("상점 타입 오류");
                return;
        }

       //_inventory.UpdateGoldAmount(price * amount * (int)_shopUI.Type);
    }

    public Item GetItemData(int index)
    {
        switch (_shopUI.Type)
        {
            case ShopType.purchase:
                return (_shopItems.Count > index && index >= 0) ? _shopItems[index] : null;

            case ShopType.sell:
                return (_inventory.GetItemsCount() > index && index >= 0) ? _inventory.GetItemByIndex(index) : null;

            default:
                Debug.Log("상점 타입 오류");
                return null;
        }
    }

    public int GetCurrentGold() => _inventory.GetCurrentGold();
}
