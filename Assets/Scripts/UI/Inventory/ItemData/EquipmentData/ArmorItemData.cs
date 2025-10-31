using System;
using UnityEngine;

public class ArmorItemData : BattleEquipmentItemData
{
    public Item_Info_Armor ArmorInfo => _armorInfo;

    [SerializeField] private Item_Info_Armor _armorInfo;

    public ArmorItemData(Item_Info_Item info) : base(info)
    {
        _armorInfo = Array.Find(Item_Info.Armor, armor => armor.id == info.id);

        _equipmentType = Equipment.EquipmentType.Armor;
    }

    public override Item Createitem()
    {
        return new ArmorItem(this);
    }

    public ArmorItemData() : base()
    {

    }
}
