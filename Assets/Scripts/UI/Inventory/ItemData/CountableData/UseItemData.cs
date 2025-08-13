using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemData : CountableItemData
{
    public Item_Info_UseItem Info { get; private set; }

    public UseItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {

    }

    public override Item Createitem() => new UseItem(this, 1);
}