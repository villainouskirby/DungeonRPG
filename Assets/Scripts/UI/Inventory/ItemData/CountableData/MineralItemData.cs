using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralItemData : ResourceItemData
{
    public MineralItemData(Item_Info_Item info) : base(info)
    {
    }

    public override Item Createitem()
    {
        return new MineralItem(this);
    }
}
