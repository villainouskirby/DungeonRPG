using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceItemData : CountableItemData
{
    public ResourceItemData(Item_Info_Item info) : base(info)
    {
    }

    public override Item Createitem()
    {
        return new ResourceItem(this);
    }
}
