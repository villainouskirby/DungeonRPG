public class SubWeaponItem : BattleEquipmentItem
{
    public SubWeaponItemData SubWeaponData => Data as SubWeaponItemData;

    public SubWeaponItem(SubWeaponItemData data) : base(data)
    {
    }

    public override Item Clone()
    {
        return new SubWeaponItem(SubWeaponData);
    }
}
