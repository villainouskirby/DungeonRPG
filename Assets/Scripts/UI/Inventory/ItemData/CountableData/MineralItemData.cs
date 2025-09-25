using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralItemData : ResourceItemData
{
    public MineralItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new MineralItem(this);
    }
}
