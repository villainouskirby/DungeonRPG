using UnityEngine;

[CreateAssetMenu(fileName = "BodyArmorData", menuName = "ItemData/EquipmentItemData/BodyArmorData")]
public class BodyArmorItemData : BattleEquipmentItemData
{
    public override Item Createitem()
    {
        return new BodyArmorItem(this);
    }
}
