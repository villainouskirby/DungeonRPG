using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResourceItem : CountableItem
{
    public ResourceItemData ResourceItemData => Data as ResourceItemData;

    public ResourceItem(ResourceItemData data, int amount = 1) : base(data, amount)
    {
    }

    public override Item Clone(int amount)
    {
        return new ResourceItem(ResourceItemData, amount);
    }
}
