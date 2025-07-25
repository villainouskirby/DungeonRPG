using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResourceItem : CountableItem
{
    public ResourceItemData ResourceItemData { get { return (ResourceItemData)_data; } private set { _data = value; } }

    public ResourceItem(ResourceItemData data, int amount = 1) : base(data, amount)
    {
        ResourceItemData = data;
    }

    public ResourceItem()
    {
    }

    public override Item Clone(int amount)
    {
        return new ResourceItem(ResourceItemData, amount);
    }
}
