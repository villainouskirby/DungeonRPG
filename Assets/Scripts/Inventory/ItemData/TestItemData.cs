using UnityEngine;

[CreateAssetMenu(fileName = "TestItemData", menuName = "ItemData/TestItemData")]
public class TestItemData : CountableItemData
{
    public override Item Createitem()
    {
        return new TestItem(this);
    }
}
