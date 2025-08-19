using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemData : CountableItemData
{
    public Item_Info_ThrowItem Info { get; private set; }

    public ThrowItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {

    }

    public override Item Createitem() => new ThrowItem(this, 1);
}