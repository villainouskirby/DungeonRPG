using System.Collections.Generic;
using UnityEngine;
using ItemDataExtensions;

[System.Serializable]
public abstract class ItemData
{
    public string SID => _info.id;
    public string Type => _info.type;
    public string Name => _info.name;
    public int Rank => _info.rank;
    public float Weight => _info.weight;
    public int SellPrice => _info.sell_price;
    public bool CanThrow => _info.throwable;
    public bool CanUse => _info.usable;
    public bool CanSetToPouch => _info.pouchable;
    public bool CanSell => _info.sellable;
    public Sprite IconSprite => _iconSprite;

    public Item_Info_Item Info => _info;
    public Dictionary<ItemDataExtension.Name, ItemDataExtension> Extensions => _extensions;

    [SerializeReference] private Item_Info_Item _info;
    [SerializeReference] private Sprite _iconSprite;
    [SerializeReference] private Dictionary<ItemDataExtension.Name, ItemDataExtension> _extensions;

    public ItemData(Item_Info_Item info, Sprite sprite)
    {
        _info = info;
        _iconSprite = sprite; // TODO => info 내부의 sprite 정보로 sprite 가져와야함 -> 하위 클래스마다 다른 경로로 하여 끌고오는게 맞을수도

        if (CanThrow)
        {
            _extensions[ItemDataExtension.Name.Throwable] = new ThrowableItemDataExtension(SID);
        }
    }

    public ItemData()
    {

    }

    /// <summary> 타입에 맞는 새로운 아이템 생성 </summary>
    public abstract Item Createitem();
}
