using UnityEngine;

[CreateAssetMenu(fileName = "LegArmorData", menuName = "ItemData/EquipmentItemData/LegArmorData")]
public class LegArmorItemData : BattleEquipmentItemData
{
    public override Item Createitem()
    {
        return new LegArmorItem(this);
    }
}
