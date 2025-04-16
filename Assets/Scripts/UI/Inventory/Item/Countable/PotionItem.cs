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

    public bool Use()
    {
        if (Amount > 0)
        {
            PotionManager.instance.GetPotionID(Data);
            Amount--;
            return true;
        }
        else return false;
    }
}
