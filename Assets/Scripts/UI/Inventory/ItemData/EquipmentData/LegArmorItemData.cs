using UnityEngine;

[CreateAssetMenu(fileName = "LegArmorData", menuName = "ItemData/EquipmentItemData/LegArmorData")]
public class LegArmorItemData : BattleEquipmentItemData
{
    public LegArmorItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new LegArmorItem(this);
    }
}
