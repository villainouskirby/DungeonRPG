public class ArmorItem : BattleEquipmentItem
{
    public ArmorItemData ArmorItemData => Data as ArmorItemData;
    public ArmorItem(ArmorItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new ArmorItem(ArmorItemData);
    }
}
