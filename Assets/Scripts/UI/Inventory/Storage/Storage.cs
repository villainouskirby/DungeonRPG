using UnityEngine;

public class Storage : Inventory
{
    [Header("Storage")]
    [SerializeField] private Inventory _storeTarget;

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    public void MoveItemToTarget(int index, int amount)
    {
        _storeTarget.AddItem(_items[index].Data, amount);
        RemoveItem(index, amount);
    }

    public override void Load(SaveData saveData)
    {
    }

    public override void Save(SaveData saveData)
    {
    }
}
