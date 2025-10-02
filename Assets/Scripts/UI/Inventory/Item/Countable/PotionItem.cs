using Cysharp.Threading.Tasks;
using Events;
using UnityEngine;

public class PotionItem : CountableItem, IUsableItem
{
    public PotionItemData PotionData => Data as PotionItemData;

    public PotionItem(PotionItemData data, int amount = 1) : base(data, amount)
    {
    }

    public override Item Clone(int amount)
    {
        return new PotionItem(PotionData, amount);
    }

    public async UniTask<bool> Use()
    {
        using (var args = InventoryBehaviorEventArgs.Get())
        {
            args.Init(InventoryBehaviorEventArgs.Behavior.Drink, Data.SID);
            EventManager.Instance.InventoryBehaviorEvent.Invoke(args);
            args.Release();
        }

        if (Amount > 0)
        {
            Amount--;
            if (!await PotionManager.Instance.GetPotionID(Data)) return false;
            return true;
        }
        else return false;
    }
}
