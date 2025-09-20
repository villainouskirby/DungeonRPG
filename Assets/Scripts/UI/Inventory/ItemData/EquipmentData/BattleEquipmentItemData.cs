using Antlr4.Runtime.Atn;
using System;
using UnityEngine;

public abstract class BattleEquipmentItemData : EquipmentItemData
{
    public int Atk => _atk;
    public float Durability => _durability;
    public float Ratio1 => _ratio1;
    public float Ratio2 => _ratio2;
    public float ChargeRatio => _chargeRatio;
    public float ChargeTime => _chargeTime;
    public float ChargeMoveSpeed => _chargeMoveSpeed;
    public float GuardRatio => _guardRatio;
    public float JustGuardRatio => _justGuardRatio;
    public float RepairCost => _repairCost;
    public string RepairItem => _repairItem;
    public int RepairItemAmount => _repairItemAmount;
    public string Explanation => _explanation;

    [SerializeField] private int _atk;
    [SerializeField] private float _durability;
    [SerializeField] private float _ratio1;
    [SerializeField] private float _ratio2;
    [SerializeField] private float _chargeRatio;
    [SerializeField] private float _chargeTime;
    [SerializeField] private float _chargeMoveSpeed;
    [SerializeField] private float _guardRatio;
    [SerializeField] private float _justGuardRatio;
    [SerializeField] private float _repairCost;
    [SerializeField] private string _repairItem;
    [SerializeField] private int _repairItemAmount;
    [SerializeField] private string _explanation;

    
    protected BattleEquipmentItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
        Item_Info_Weapon rawWeaponData = Array.Find(Item_Info.Weapon, weapon => weapon.id == info.id);

        _atk = rawWeaponData.atk;
        _durability = rawWeaponData.durability;
        _ratio1 = rawWeaponData.ratio1;
        _ratio2 = rawWeaponData.ratio2;
        _chargeRatio = rawWeaponData.strong_Ratio;
        _chargeTime = rawWeaponData.max_charge;
        _chargeMoveSpeed = rawWeaponData.strong_speed;
        _guardRatio = rawWeaponData.guard_ratio;
        _justGuardRatio = rawWeaponData.just_guard;
        _repairCost = rawWeaponData.repair_cost;
        _repairItem = rawWeaponData.repair_ing;
        _repairItemAmount = rawWeaponData.repair_ing_count;
        _explanation = rawWeaponData.Explanation;
    }
}
