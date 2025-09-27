public class BackpackItem : EquipmentItem
{
    public BackpackItemData BagData => Data as BackpackItemData;

    public BackpackItem(BackpackItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new BackpackItem(BagData);
    }
}
