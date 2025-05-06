using Cysharp.Threading.Tasks;

public abstract class EquipmentItem : Item, ISyncUsableItem
{
    public EquipmentItemData EquipmentData { get; private set; }

    public EquipmentItem(EquipmentItemData data) : base(data)
    {
        EquipmentData = data;
    }

    public bool UseSync() { return true; }

    public UniTask<bool> Use() => UniTask.FromResult(UseSync());
}
