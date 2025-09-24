public class ToolItem : EquipmentItem
{
    public ToolItemData ToolData => Data as ToolItemData;

    public ToolItem(ToolItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new ToolItem(ToolData);
    }
}
