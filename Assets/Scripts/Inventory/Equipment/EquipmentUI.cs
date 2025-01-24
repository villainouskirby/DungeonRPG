using System;
using UnityEngine;

public class EquipmentUI : SlotInteractHandler
{
    [SerializeField] private Equipment _equipment;
    [SerializeField] private EquipmentPopUpUI _equipmentPopUpUI;
    [SerializeField] private EquipmentEffectUI _equipmentEffectUI;

    [Header("SlotUIs")]
    [SerializeField] private EquipmentSlotUI _weaponSlot;
    [SerializeField] private EquipmentSlotUI _toolSlot;
    [SerializeField] private EquipmentSlotUI _bodyArmorSlot;
    [SerializeField] private EquipmentSlotUI _legArmorSlot;

    public void SetEquipmentSlot(EquipmentType type, Sprite iconSprite)
    {
        SlotUI slot = type switch
        {
            EquipmentType.weapon => _weaponSlot,
            EquipmentType.tool => _toolSlot,
            EquipmentType.bodyArmor => _bodyArmorSlot,
            EquipmentType.legArmor => _legArmorSlot,
            _ => _toolSlot
        };

        slot.SetItemInfo(iconSprite);
    }

    public void SetEquipmentEffect()
    {
        _equipmentEffectUI.SetEffectTexts();
    }

    /// <returns> 타입에 맞는 현재 장착하고 있는 장비 데이터 </returns>
    public ItemData GetItemData(EquipmentType type)
    {
        return _equipment.GetItemData(type);
    }

    /// <returns> 해당 슬롯의 장비 타입 </returns>
    private EquipmentType GetSlotEquipmentType(EquipmentSlotUI slot)
    {
        return slot.EquipmentType;
    }

    #region Pointer Event

    public override void OnDoubleClick()
    {
        _equipment.UnEquip(GetSlotEquipmentType(_pointedSlot as EquipmentSlotUI));
    }

    public override void OnRightClick()
    {
        
    }

    public override void OnPointerIn()
    {
        _equipmentPopUpUI.OpenInfo(GetSlotEquipmentType(_pointedSlot as EquipmentSlotUI));
    }

    public override void OnPointerOut()
    {
        _equipmentPopUpUI.CloseInfo();
    }

    #endregion
}
