using Cysharp.Threading.Tasks;

public abstract class EquipmentItem : Item, IUsableItem
{
    public EquipmentItemData EquipmentData { get; private set; }

    public EquipmentItem(EquipmentItemData data) : base(data)
    {
        EquipmentData = data;
    }

    public bool Use() { return true; }
}
