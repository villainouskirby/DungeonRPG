public class LegArmorItem : BattleEquipmentItem
{
    public LegArmorItemData LegArmorData { get; private set; }

    public LegArmorItem(LegArmorItemData data) : base(data)
    {
        LegArmorData = data;
    }

    public override Item Clone()
    {
        return new LegArmorItem(LegArmorData);
    }
}
