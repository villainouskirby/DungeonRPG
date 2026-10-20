using UnityEngine;

public class TestItemData : CountableItemData
{
    public TestItemData(Item_Info_Item info) : base(info)
    {
    }

    public override Item Createitem()
    {
        return new TestItem(this);
    }
}
