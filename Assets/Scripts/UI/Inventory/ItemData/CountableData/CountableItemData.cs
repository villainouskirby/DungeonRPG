using UnityEngine;

/// <summary> 셀 수 있는 아이템 데이터 </summary>
public abstract class CountableItemData : ItemData   
{
    public int MaxAmount => Info.max_amount;

    protected CountableItemData(Item_Info_Item info) : base(info)
    {
    }

    public CountableItemData()
    {

    }
}
