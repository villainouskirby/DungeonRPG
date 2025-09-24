using UnityEngine;

public class HerbItemData : ResourceItemData
{
    public HerbItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new HerbItem(this);
    }
}
