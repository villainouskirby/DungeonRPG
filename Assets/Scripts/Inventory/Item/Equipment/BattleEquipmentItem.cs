public abstract class BattleEquipmentItem : EquipmentItem
{
    public BattleEquipmentItemData BattleEquipmentData { get; private set; }
    protected BattleEquipmentItem(BattleEquipmentItemData data) : base(data)
    {
        BattleEquipmentData = data;
    }
}
