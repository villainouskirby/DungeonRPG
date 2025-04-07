public class WeaponItem : BattleEquipmentItem
{
    public WeaponItemData WeaponData {  get; private set; }
    public WeaponItem(WeaponItemData data) : base(data)
    {
        WeaponData = data;
    }

    public override Item Clone()
    {
        return new WeaponItem(WeaponData);
    }
}
