using Antlr4.Runtime.Atn;
using System;
using UnityEngine;

public abstract class BattleEquipmentItemData : EquipmentItemData
{
    protected BattleEquipmentItemData(Item_Info_Item info) : base(info)
    {
    }

    public BattleEquipmentItemData() : base()
    {

    }
}
