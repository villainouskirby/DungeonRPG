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

    [SerializeField] private FloatVariableSO _hpSO;
    [SerializeField] private FloatVariableSO _staminaSO;
    [SerializeField] private FloatVariableSO _attackSO;
    [SerializeField] private FloatVariableSO _speedSO;

    private Dictionary<EquipmentType, EquipmentItem> _playerEquipments = new Dictionary<EquipmentType, EquipmentItem>();

    protected override void OnDisable()
    {
        
    }

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _isActvieOnStart = true;

        RefreshAttackGateByWeapon();
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

        if (type == EquipmentType.Backpack)
        {
            UIPopUpHandler.Instance.GetScript<QuickSlot>().InitQuickSlot();
        }

        if (type == EquipmentType.Weapon) RefreshAttackGateByWeapon();
    }

    public void UnEquip(EquipmentType type)
    {
        EquipmentItem ei;
        if (!_playerEquipments.TryGetValue(type, out ei)) return;

        ei.IsEquipped = false;
        UpdateEquipmentEffect(type, false);
        _playerEquipments.Remove(type);
        UpdateSlot(type);

        if (type == EquipmentType.Weapon) RefreshAttackGateByWeapon();
    }

    /// <summary>
    /// 장비효과 업데이트 
    /// <para/><paramref name="isPlus"/> => true이면 덧셈, false이면 뺄셈
    /// </summary>
    private void UpdateEquipmentEffect(EquipmentType type, bool isPlus = true) // 문제는 없을텐데 기존 스탯에서 장착한 장비들의 추가값 더하는 식으로 바꿔야할듯
    {
        int sign = isPlus ? 1 : -1;
        var data = _playerEquipments[type].Data;

        switch (type)
        {
            case EquipmentType.Weapon:
                _attackSO.Value += sign * (data as WeaponItemData).WeaponInfo.atk;
                break;

            case EquipmentType.SubWeapon:
                break;

            case EquipmentType.Armor:
                Item_Info_Armor armorInfo = (data as ArmorItemData).ArmorInfo;
                _hpSO.Value += sign * armorInfo.hp;
                _staminaSO.Value += sign * armorInfo.stamina;
                _speedSO.Value += sign * armorInfo.speed;
                break;

            case EquipmentType.Backpack:
                Item_Info_Backpack backpackInfo = (data as BackpackItemData).BackpackInfo;
                _inventory.ChangeMaxCapacity(backpackInfo.max_weight);
                _speedSO.Value += sign * backpackInfo.speed;
                break;
        }

        _equipmentUI.SetEquipmentEffect();
    }

    /// <summary>
    /// 무기 장착 여부에 따라 전역 공격, 가드 허용을 토글
    /// </summary>
    private void RefreshAttackGateByWeapon()
    {
        bool hasWeapon = _playerEquipments.ContainsKey(EquipmentType.Weapon);
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SetAttack(hasWeapon);
            PlayerManager.Instance.SetGuard(hasWeapon);
        }
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

        RefreshAttackGateByWeapon();
    }

    public void Save(SaveData saveData)
    {
        saveData.Equipments = _playerEquipments;
    }
}
