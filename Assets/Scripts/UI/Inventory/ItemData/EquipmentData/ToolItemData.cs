using UnityEngine;

[CreateAssetMenu(fileName = "ToolData", menuName = "ItemData/EquipmentItemData/ToolData")]
public class ToolItemData : EquipmentItemData
{
    public override Item Createitem()
    {
        return new ToolItem(this);
    }
}
