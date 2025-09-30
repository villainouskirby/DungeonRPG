using Cysharp.Threading.Tasks;

public class ThrowItem : CountableItem, IUsableItem
{
    public ThrowItemData ThrowData { get; }

    public ThrowItem(ThrowItemData data, int amount = 1) : base(data, amount)
    {
        ThrowData = data;
    }

    public override Item Clone(int amount) => new ThrowItem(ThrowData, amount);

    public async UniTask<bool> Use()
    {
        if (Amount <= 0) return false;

        // 포션과 동일한 싱글톤 호출 방식
        if (!await ThrowItemManager.Instance.UseThrowItem(ThrowData))
            return false;

        Amount--;
        return true;
    }
}