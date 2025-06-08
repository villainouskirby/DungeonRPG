using UnityEngine;

[CreateAssetMenu(fileName = "BodyArmorData", menuName = "ItemData/EquipmentItemData/BodyArmorData")]
public class BodyArmorItemData : BattleEquipmentItemData
{
    public BodyArmorItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public override Item Createitem()
    {
        return new BodyArmorItem(this);
    }
}
