public class HerbItem : ResourceItem
{
    public HerbItemData HerbData => Data as HerbItemData;

    public HerbItem(ResourceItemData data, int amount = 1) : base(data, amount)
    {
    }

    public override Item Clone(int amount)
    {
        return new HerbItem(HerbData, amount);
    }
}
