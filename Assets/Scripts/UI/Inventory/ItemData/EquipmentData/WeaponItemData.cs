using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ItemData/EquipmentItemData/WeaponData")]
public class WeaponItemData : BattleEquipmentItemData
{
    public WeaponItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new WeaponItem(this);
    }
}
