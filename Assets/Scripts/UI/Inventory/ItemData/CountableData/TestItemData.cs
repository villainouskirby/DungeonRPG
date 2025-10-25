using UnityEngine;

[CreateAssetMenu(fileName = "TestData", menuName = "ItemData/CountableItem/TestItemData")]
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
