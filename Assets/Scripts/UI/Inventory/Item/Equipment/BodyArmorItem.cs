public class BodyArmorItem : BattleEquipmentItem
{
    public BodyArmorItemData BodyArmorItemData => Data as BodyArmorItemData;
    public BodyArmorItem(BodyArmorItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new BodyArmorItem(BodyArmorItemData);
    }
}
