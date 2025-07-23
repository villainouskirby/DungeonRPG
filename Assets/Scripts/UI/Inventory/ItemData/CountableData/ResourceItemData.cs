using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceItemData : CountableItemData
{
    public ResourceItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public ResourceItemData()
    {

    }

    public override Item Createitem()
    {
        return new ResourceItem(this);
    }
}
