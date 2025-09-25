using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ItemData/EquipmentItemData/WeaponData")]
public class WeaponItemData : BattleEquipmentItemData
{
    public int Atk => _weaponInfo.atk;
    public float Durability => _weaponInfo.durability;
    public float Ratio1 => _weaponInfo.ratio1;
    public float Ratio2 => _weaponInfo.ratio2;
    public float ChargeRatio => _weaponInfo.strong_Ratio;
    public float ChargeTime => _weaponInfo.max_charge;
    public float ChargeMoveSpeed => _weaponInfo.strong_speed;
    public float GuardRatio => _weaponInfo.guard_ratio;
    public float JustGuardRatio => _weaponInfo.just_guard;
    public float RepairCost => _weaponInfo.repair_cost;
    public string RepairItem => _weaponInfo.repair_ing;
    public int RepairItemAmount => _weaponInfo.repair_ing_count;
    public Item_Info_Weapon WeaponInfo => _weaponInfo;

    [SerializeField] private Item_Info_Weapon _weaponInfo;

    public WeaponItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
        _weaponInfo = Array.Find(Item_Info.Weapon, weapon => weapon.id == info.id);

        _equipmentType = EquipmentType.weapon;
    }

    public override Item Createitem()
    {
        return new WeaponItem(this);
    }
}
