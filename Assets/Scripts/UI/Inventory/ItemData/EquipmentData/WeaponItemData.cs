using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ItemData/EquipmentItemData/WeaponData")]
public class WeaponItemData : BattleEquipmentItemData
{
    public override Item Createitem()
    {
        return new WeaponItem(this);
    }
}
