public class PotionItem : CountableItem
{
    public PotionItemData PotionData { get; private set; }
    public PotionItem(PotionItemData data, int amount = 1) : base(data, amount)
    {
        PotionData = data;
    }

    public override Item Clone(int amount = 1)
    {
        return new PotionItem(PotionData, amount);
    }
}
