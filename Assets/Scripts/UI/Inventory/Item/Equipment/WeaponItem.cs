public class WeaponItem : BattleEquipmentItem
{
    public WeaponItemData WeaponData => Data as WeaponItemData;
    public WeaponItem(WeaponItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new WeaponItem(WeaponData);
    }
}
