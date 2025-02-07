using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData testItemData;
    [SerializeField] private int amount = 1;

    [SerializeField] private ItemData[] allEquipment;

    [SerializeField] private ShopItemsSO shopItems;

    public void AddInventory()
    {
        AddInventory(testItemData);
    }

    public void AddInventory(ItemData data)
    {
        int rest = inventory.AddItem(data, amount);
        if (rest > 0) Debug.Log("들어가지 못한 아이템 개수 : " + rest);
    }

    public void AddAllEquipment()
    {
        foreach (ItemData data in allEquipment)
        {
            AddInventory(data);
        }
    }

    public void AddShopItem()
    {
        shopItems.Items.Add(new ShopItem(testItemData, 10));
    }
}
