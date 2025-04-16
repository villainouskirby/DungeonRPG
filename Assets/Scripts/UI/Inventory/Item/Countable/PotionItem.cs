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
                // ID에 따른 버프 종류는 BuffManager에서 할당
                BuffManager.instance.CreateBuff(Data.ID, ((PotionItemData)Data).Percentage, ((PotionItemData)Data).Duration, Data.IconSprite);
                
            }
            else if (Data.ID <= 20) // 11~20은 HP 회복 포션
            {
                PlayerData.instance.HPValueChange(((PotionItemData)Data).Healamount);
            }
            Amount--;
            return true;
        }
        else return false;
    }
}
