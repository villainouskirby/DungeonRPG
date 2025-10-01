using Cysharp.Threading.Tasks;
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
        if (Amount > 0)
        {
            if (!await PotionManager.Instance.GetPotionID(Data)) return false;
            Amount--;
            return true;
        }
        else return false;
    }
}
