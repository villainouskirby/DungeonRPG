using UnityEngine;

[CreateAssetMenu(fileName = "ToolData", menuName = "ItemData/EquipmentItemData/ToolData")]
public class ToolItemData : EquipmentItemData
{
    public ToolItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new ToolItem(this);
    }
}
