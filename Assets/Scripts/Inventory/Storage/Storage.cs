using UnityEngine;

public class Storage : Inventory
{
    [Header("Storage")]
    [SerializeField] private Inventory _storeTarget;

    public void MoveItemToTarget(int index, int amount)
    {
        _storeTarget.AddItem(_items[index].Data, amount);
        RemoveItem(index, amount);
    }
}
