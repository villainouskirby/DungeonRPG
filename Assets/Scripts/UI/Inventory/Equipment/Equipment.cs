using System;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : UIBase, ISave
{
    public enum EquipmentType
    {
        Weapon,
        SubWeapon,
        Armor,
        Backpack
    }

    [SerializeField] private EquipmentUI _equipmentUI;
    [SerializeField] private Inventory _inventory;

    [SerializeField] private EquipmentEffectSO _equipmentEffectSO;

    private Dictionary<EquipmentType, EquipmentItem> _playerEquipments = new Dictionary<EquipmentType, EquipmentItem>();

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    public void Equip(EquipmentItem equipmentItem)
    {
        EquipmentType type = equipmentItem.EquipmentData.EquipmentType;
        if (_playerEquipments.ContainsKey(type))
        {
            UnEquip(type);
        }

        _playerEquipments[type] = equipmentItem;
        equipmentItem.IsEquipped = true;

        UpdateSlot(type);
        UpdateEquipmentEffect(type);
    }

    public void UnEquip(EquipmentType type)
    {
        EquipmentItem ei;
        if (!_playerEquipments.TryGetValue(type, out ei)) return;

        ei.IsEquipped = false;
        UpdateEquipmentEffect(type, false);
        _playerEquipments.Remove(type);
        UpdateSlot(type);
    }

    /// <summary>
    /// 장비효과 업데이트 
    /// <para/><paramref name="isPlus"/> => true이면 덧셈, false이면 뺄셈
    /// </summary>
    private void UpdateEquipmentEffect(EquipmentType type, bool isPlus = true) // 문제는 없을텐데 기존 스탯에서 장착한 장비들의 추가값 더하는 식으로 바꿔야할듯
    {
        if (type == EquipmentType.Backpack) return;

        BattleEquipmentItemData beiData = _playerEquipments[type].Data as BattleEquipmentItemData;
        int sign = isPlus ? 1 : -1;
        /*
        _equipmentEffectSO.Damage += beiData.Atk * sign;
        _equipmentEffectSO.Hp += beiData.Durability * sign;
        _equipmentEffectSO.Stamina += beiData.Stamina * sign;

        if (isPlus) _equipmentEffectSO.AdditionalEffects.Add(beiData.AdditionalEffect);
        else _equipmentEffectSO.AdditionalEffects.Remove(beiData.AdditionalEffect);*/

        _equipmentUI.SetEquipmentEffect();
    }

    public void UpdateSlot(EquipmentType type)
    {
        EquipmentItem ei;
        if (_playerEquipments.TryGetValue(type, out ei))
        {
            _equipmentUI.SetEquipmentSlot(type, ei.Data.IconSprite);
        }
        else
        {
            _equipmentUI.SetEquipmentSlot(type, null);
        }
    }

    /// <returns> 타입에 맞는 현재 장착하고 있는 장비 데이터 </returns>
    public ItemData GetItemData(EquipmentType type)
    {
        return (_playerEquipments.ContainsKey(type)) ? _playerEquipments[type].Data : null;
    }

    public void Load(SaveData saveData)
    {
        _playerEquipments = saveData.Equipments;
    }

    public void Save(SaveData saveData)
    {
        saveData.Equipments = _playerEquipments;
    }
}
