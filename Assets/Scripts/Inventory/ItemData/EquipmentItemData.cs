using UnityEngine;

/// <summary> 장비 데이터 </summary>
public abstract class EquipmentItemData : ItemData
{
    public int Durability => _durability;

    [SerializeField] private int _durability;
}
