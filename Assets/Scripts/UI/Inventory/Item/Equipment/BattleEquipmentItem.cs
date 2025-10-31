public abstract class BattleEquipmentItem : EquipmentItem
{
    public BattleEquipmentItemData BattleEquipmentData => Data as BattleEquipmentItemData;
    protected BattleEquipmentItem(BattleEquipmentItemData data) : base(data)
    {
    }

    public BattleEquipmentItem() : base()
    {

    }
}
