public class BodyArmorItem : BattleEquipmentItem
{
    public BodyArmorItemData BodyArmorItemData { get; private set; }
    public BodyArmorItem(BodyArmorItemData data) : base(data)
    {
        BodyArmorItemData = data;
    }

    public override Item Clone()
    {
        return new BodyArmorItem(BodyArmorItemData);
    }
}
