using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    [SerializeField] private EquipmentUI _equipmentUI;
    [SerializeField] private Inventory _inventory;

    [SerializeField] private EquipmentEffectSO _equipmentEffectSO;

    private Dictionary<EquipmentType, EquipmentItem> _playerEquipments = new Dictionary<EquipmentType, EquipmentItem>();

    public void Equip(EquipmentItemData equipmentItemData)
    {
        EquipmentType type = equipmentItemData.EquipmentType;
        if (_playerEquipments.ContainsKey(type))
        {
            UnEquip(type);
        }

        _playerEquipments[type] = equipmentItemData.Createitem() as EquipmentItem;
        UpdateSlot(type);
        UpdateEquipmentEffect(type);
    }

    public void UnEquip(EquipmentType type)
    {
        EquipmentItem ei;
        if (!_playerEquipments.TryGetValue(type, out ei)) return;

        // 인벤에 넣은 후 장비창에서 제거
        _inventory.AddItem(ei.Data);

        UpdateEquipmentEffect(type, false);
        _playerEquipments.Remove(type);
        UpdateSlot(type);
    }

    /// <summary>
    /// 장비효과 업데이트 
    /// <para/><paramref name="isPlus"/> => true이면 덧셈, false이면 뺄셈
    /// </summary>
    private void UpdateEquipmentEffect(EquipmentType type, bool isPlus = true)
    {
        if (type == EquipmentType.tool) return;

        BattleEquipmentItemData beiData = _playerEquipments[type].Data as BattleEquipmentItemData;
        int sign = isPlus ? 1 : -1;

        _equipmentEffectSO.Damage += beiData.Damage * sign;
        _equipmentEffectSO.Hp += beiData.Hp * sign;
        _equipmentEffectSO.Stamina += beiData.Stamina * sign;

        if (isPlus) _equipmentEffectSO.AdditionalEffects.Add(beiData.AdditionalEffect);
        else _equipmentEffectSO.AdditionalEffects.Remove(beiData.AdditionalEffect);

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
}
