using UnityEngine;

[CreateAssetMenu(fileName = "TestData", menuName = "ItemData/CountableItem/TestItemData")]
public class TestItemData : CountableItemData
{
    public TestItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new TestItem(this);
    }
}
