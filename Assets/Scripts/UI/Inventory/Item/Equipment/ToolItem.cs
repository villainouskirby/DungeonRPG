public class ToolItem : EquipmentItem
{
    public ToolItemData ToolData { get; private set; }

    public ToolItem(ToolItemData data) : base(data)
    {
        ToolData = data;
    }

    public override Item Clone()
    {
        return new ToolItem(ToolData);
    }
}
