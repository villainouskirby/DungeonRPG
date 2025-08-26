using UnityEngine;

public class StorageUI : InventoryUI
{
    [Header("Storage")]
    [SerializeField] private Storage _storage;

    public override void OnDoubleClick()
    {
        int index = GetItemSlotIndex(_pointedSlot as InventoryItemSlotUI);
        _storage.MoveItemToTarget(index, _storage.GetItemAmount(index));
    }

    public override void OnRightClick()
    {
        int index = GetItemSlotIndex(_pointedSlot as InventoryItemSlotUI);
        _storage.MoveItemToTarget(index, 1);
    }
}
