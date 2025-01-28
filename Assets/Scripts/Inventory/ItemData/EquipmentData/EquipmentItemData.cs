using UnityEngine;

/// <summary> 장비 데이터 </summary>
public abstract class EquipmentItemData : ItemData
{
    public EquipmentType EquipmentType => _equipmentType;
    public int Rank => _rank;

    [SerializeField] private EquipmentType _equipmentType;
    [SerializeField] private int _rank;
}
