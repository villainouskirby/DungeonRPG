using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class EquipmentItem : Item, ISyncUsableItem
{
    public Action<bool> OnEquippedChanged;

    public EquipmentItemData EquipmentData => Data as EquipmentItemData;
    
    public bool IsEquipped
    {
        get => _isEquipped;
        set
        {
            _isEquipped = value;
            OnEquippedChanged?.Invoke(value);
        }
    }
    [SerializeField] private bool _isEquipped = false;

    public EquipmentItem(EquipmentItemData data) : base(data)
    {
    }

    public bool UseSync() { return true; }

    public UniTask<bool> Use()
    {
        if (IsEquipped) return UniTask.FromResult(false);

        return UniTask.FromResult(UseSync());
    }
}
