using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class EquipmentItem : Item, ISyncUsableItem
{
    public EquipmentItemData EquipmentData => Data as EquipmentItemData;
    public bool IsEquipped = false;

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
