using System;
using UnityEngine;

public class BackpackItemData : EquipmentItemData
{
    public Item_Info_Backpack BackpackInfo => _backpackInfo;

    [SerializeField] private Item_Info_Backpack _backpackInfo;

    public BackpackItemData(Item_Info_Item info) : base(info)
    {
        _backpackInfo = Array.Find(Item_Info.Backpack, backpack => backpack.id == info.id);

        _equipmentType = Equipment.EquipmentType.Backpack;
    }

    public override Item Createitem()
    {
        return new BackpackItem(this);
    }

    public BackpackItemData() : base()
    {

    }
}
