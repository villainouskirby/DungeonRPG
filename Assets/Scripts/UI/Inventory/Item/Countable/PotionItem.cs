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
        //추가 : 스크립터블 오브젝트 ID를 읽어서 포션 버프를 활성화
        switch (Data.ID)          // 스크립터블 오브젝트 인스펙터에서 _id = 1, 2 … 지정
        {
            case 1: // Attack Up 포션
                BuffManager.instance.CreateBuff(BuffType.AttackUp,percentage: 0.4f,duration: 10f,Data.IconSprite);
                break;

            case 2: // Attack Down 포션
                BuffManager.instance.CreateBuff(BuffType.AttackDown,percentage: 0.3f,duration: 10f, Data.IconSprite);
                break;

            default:
                return false;     // 사용 실패
        }
        //추가

        if (Amount > 0)
        {
            Amount--;
            return true;
        }
        else return false;
    }
}
