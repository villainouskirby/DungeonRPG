using Cysharp.Threading.Tasks;

public class PotionItem : CountableItem, IUsableItem
{
    public PotionItemData PotionData { get; private set; }
    public PotionItem(PotionItemData data, int amount = 1) : base(data, amount)
    {
        PotionData = data;
    }

    public override Item Clone(int amount)
    {
        return new PotionItem(PotionData, amount);
    }

    public async UniTask<bool> Use()
    {
        if (Amount > 0)
        {
            if (!await PotionManager.instance.GetPotionID(Data)) return false;

            Amount--;
            return true;
        }
        else return false;
    }
}
