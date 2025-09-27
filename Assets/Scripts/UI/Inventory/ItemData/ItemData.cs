using System.Collections.Generic;
using UnityEngine;
using ItemDataExtensions;

[System.Serializable]
public abstract class ItemData
{
    public string SID => _info.id;
    public string Name => _info.name;
    public Sprite IconSprite => _iconSprite;

    public Item_Info_Item Info => _info;
    public Dictionary<ItemDataExtension.Name, ItemDataExtension> Extensions => _extensions;

    [SerializeReference] private Item_Info_Item _info;
    [SerializeReference] private Dictionary<ItemDataExtension.Name, ItemDataExtension> _extensions = new();

    [SerializeField] private Sprite _iconSprite;

    public ItemData(Item_Info_Item info, Sprite sprite)
    {
        _info = info;
        _iconSprite = sprite; // TODO => info 내부의 sprite 정보로 sprite 가져와야함 -> 하위 클래스마다 다른 경로로 하여 끌고오는게 맞을수도

        if (_info.throwable)
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
