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
            BuffManager.instance.CreateBuff(Data.ID, percentage: 0.4f, duration: 10f, Data.IconSprite);
            Amount--;
            return true;
        }
        else return false;
    }
}
