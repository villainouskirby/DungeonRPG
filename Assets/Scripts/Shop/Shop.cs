using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    private List<ShopItem> _items = new List<ShopItem>();

    [SerializeField] private TestItemData testData; // 테스트용
    [SerializeField] private Button button;
    private void Start()
    {
        _items.Add(new ShopItem(testData.Createitem(), 30, 30));
        button.onClick.AddListener(() => Trade());
    }

    public void AddItem(ShopItem item)
    {
        _items.Add(item);
    }

    public IEnumerable<ShopItem> GetAvailableItems()
    {
        return _items.Where(item => item.IsAvailable);
    }

    public ShopItem GetItemById(int itemId)
    {
        return _items.FirstOrDefault(item => item.Item.Data.ID == itemId);
    }

    public Item Trade() // TODO => 상점탭에서 클릭한 템의 정보 가져와서 구매하도록 연동시키기
    {
        ShopItem shopItem = GetItemById(0);
        if (shopItem == null) return null;
        return shopItem.Trade(1);
    }
    
}
