using System;
using UnityEngine;

public class WeaponItemData : BattleEquipmentItemData
{
    public Item_Info_Weapon WeaponInfo => _weaponInfo;

    [SerializeField] private Item_Info_Weapon _weaponInfo;

    public WeaponItemData(Item_Info_Item info) : base(info)
    {
        _weaponInfo = Array.Find(Item_Info.Weapon, weapon => weapon.id == info.id);

        _equipmentType = Equipment.EquipmentType.Weapon;
    }

    public override Item Createitem()
    {
        return new WeaponItem(this);
    }
}
