using Cysharp.Threading.Tasks;

public class ResourceItem : CountableItem
{
    public ResourceItemData ResourceData { get; private set; }
    public ResourceItem(ResourceItemData data, int amount = 1) : base(data, amount)
    {
        ResourceData = data;
    }

    public override Item Clone(int amount)
    {
        return new ResourceItem(ResourceData, amount);
    }
}
