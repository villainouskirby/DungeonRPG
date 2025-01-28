using UnityEngine;

[CreateAssetMenu(fileName = "TestData", menuName = "ItemData/CountableItem/TestItemData")]
public class TestItemData : CountableItemData
{
    public override Item Createitem()
    {
        return new TestItem(this);
    }
}
