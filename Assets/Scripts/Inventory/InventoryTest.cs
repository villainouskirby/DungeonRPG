using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData testItemData;
    [SerializeField] private int amount = 1;

    public void AddInventory()
    {
        int rest = inventory.AddItem(testItemData, amount);
        if (rest > 0) Debug.Log("들어가지 못한 아이템 개수 : " + rest);
    }
}
