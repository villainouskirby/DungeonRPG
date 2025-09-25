public class LegArmorItem : BattleEquipmentItem
{
    public LegArmorItemData LegArmorData => Data as LegArmorItemData;

    public LegArmorItem(LegArmorItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new LegArmorItem(LegArmorData);
    }
}
