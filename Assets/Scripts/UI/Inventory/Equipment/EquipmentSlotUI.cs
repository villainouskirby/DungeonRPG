using UnityEngine;

public class EquipmentSlotUI : SlotUI
{
    /// <summary> 장착할 장비 타입 </summary>
    public EquipmentType EquipmentType => _equipmentType;

    [Header("장착할 장비 타입")]
    [SerializeField] private EquipmentType _equipmentType;
}
