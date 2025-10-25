using UnityEngine;

public class HerbItemData : ResourceItemData
{
    public HerbItemData(Item_Info_Item info) : base(info)
    {
    }

    public override Item Createitem()
    {
        return new HerbItem(this);
    }
}
