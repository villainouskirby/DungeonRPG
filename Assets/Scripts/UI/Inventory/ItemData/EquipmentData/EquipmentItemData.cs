using UnityEngine;

/// <summary> 장비 데이터 </summary>
public abstract class EquipmentItemData : ItemData
{
    public EquipmentType EquipmentType => _equipmentType;

    [SerializeField] protected EquipmentType _equipmentType;

    protected EquipmentItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }
}
