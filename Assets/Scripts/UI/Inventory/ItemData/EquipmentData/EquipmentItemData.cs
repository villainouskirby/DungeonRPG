using UnityEngine;

/// <summary> 장비 데이터 </summary>
public abstract class EquipmentItemData : ItemData
{
    public Equipment.EquipmentType EquipmentType => _equipmentType;

    [SerializeField] protected Equipment.EquipmentType _equipmentType;
    [SerializeField] private bool _isEquipped;

    protected EquipmentItemData(Item_Info_Item info) : base(info)
    {
    }

    public EquipmentItemData() : base()
    {
    }
}
