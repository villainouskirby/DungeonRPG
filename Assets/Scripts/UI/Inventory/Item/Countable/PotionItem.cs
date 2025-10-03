using Cysharp.Threading.Tasks;
using Events;
using UnityEngine;

public class PotionItem : CountableItem, IUsableItem
{
    public PotionItemData PotionData => Data as PotionItemData;

    public PotionItem(PotionItemData data, int amount = 1) : base(data, amount)
    {
    }

    public override Item Clone(int amount)
    {
        return new PotionItem(PotionData, amount);
    }

    public async UniTask<bool> Use()
    {
        using (var args = InventoryBehaviorEventArgs.Get())
        {
            args.Init(InventoryBehaviorEventArgs.Behavior.Drink, Data.SID);
            EventManager.Instance.InventoryBehaviorEvent.Invoke(args);
            args.Release();
        }

        var result = await PotionManager.Instance.GetPotionID(Data);

        switch (result)
        {
            case PotionUseResult.FailedToStart:
                // 예: HP 풀, 잘못된 데이터 등 → 소모 안함
                return false;

            case PotionUseResult.Completed:
                Amount--;               // 정상 종료 → 소모
                return true;

            case PotionUseResult.Cancelled:
                Amount--;               // 회피/피격으로 중간 취소 → 소모
                return false;
        }
        return false;
    }
}
public enum PotionUseResult
{
    FailedToStart, // 시작 불가(HP 풀, 잘못된 데이터 등)
    Completed,     // 정상 종료
    Cancelled      // 중간 취소(회피/피격 등)
}
