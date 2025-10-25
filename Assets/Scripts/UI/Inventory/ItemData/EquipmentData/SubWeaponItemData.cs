using System;
using UnityEngine;

public class SubWeaponItemData : BattleEquipmentItemData
{
    public Item_Info_SubWeapon SubWeaponInfo => _subWeaponInfo;

    [SerializeField] private Item_Info_SubWeapon _subWeaponInfo;

    public SubWeaponItemData(Item_Info_Item info) : base(info)
    {
        _subWeaponInfo = Array.Find(Item_Info.SubWeapon, subWeapon => subWeapon.id == info.id);

        _equipmentType = Equipment.EquipmentType.SubWeapon;
    }

    public override Item Createitem()
    {
        return new SubWeaponItem(this);
    }
}
