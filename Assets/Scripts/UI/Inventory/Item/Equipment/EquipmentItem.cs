using Cysharp.Threading.Tasks;

public abstract class EquipmentItem : Item, ISyncUsableItem
{
    public EquipmentItemData EquipmentData => Data as EquipmentItemData;

    public EquipmentItem(EquipmentItemData data) : base(data)
    {
    }

    public bool UseSync() { return true; }

    public UniTask<bool> Use() => UniTask.FromResult(UseSync());

}
