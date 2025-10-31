using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class EquipmentItem : Item, ISyncUsableItem
{
    [NonSerialized] public Action<bool> OnEquippedChanged;

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

    public EquipmentItem() : base()
    {

    }

    public override Item Clone()
    {
        return null;
    }

    public bool UseSync() { return true; }

    public UniTask<bool> Use()
    {
        if (IsEquipped) return UniTask.FromResult(false);

        return UniTask.FromResult(UseSync());
    }
}
