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
            if (Data.ID <= 10) // 1~10은 버프 포션
            {
                BuffManager.instance.CreateBuff(Data.ID, percentage: 0.4f, duration: 10f, Data.IconSprite);
                //TODO : 스크립터블 오브젝트에서 포션 데이터 얻는 로직 고민해보기
            }
            else if (Data.ID <= 20) // 11~20은 HP 회복 포션
            {
                PlayerData.instance.HPValueChange(10f); //TODO : 스크립터블 오브젝트에서 힐량 얻기
            }
            Amount--;
            return true;
        }
        else return false;
    }
}
