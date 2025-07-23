using UnityEngine;

/// <summary> 셀 수 있는 아이템 데이터 </summary>
public abstract class CountableItemData : ItemData   
{
    public int MaxAmount => _maxAmount;

    [SerializeField] private int _maxAmount = 99;

    protected CountableItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    public CountableItemData()
    {

    }
}
