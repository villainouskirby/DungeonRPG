using Cysharp.Threading.Tasks;

public class MineralItem : ResourceItem, IUsableItem
{
    public MineralItemData MineralItemData => Data as MineralItemData;

    public MineralItem(MineralItemData data, int amount = 1) : base(data, amount) 
    { 
    }


    public override Item Clone(int amount = 1)
    {
        return new MineralItem(MineralItemData, amount);
    }

    public async UniTask<bool> Use()
    {
        if (Amount <= 0) return false;

        // 포션과 동일한 싱글톤 호출 방식
        if (Data.Info.throwable && !await ThrowItemManager.instance.UseThrowItem(Data))
            return false;

        Amount--;
        return true;
    }
}
