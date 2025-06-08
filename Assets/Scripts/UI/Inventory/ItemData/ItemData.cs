using UnityEngine;

[System.Serializable]
public abstract class ItemData
{
    public int ID => _id;
    public string Name => _name;
    public string Tooltip => _tooltip;
    public int Weight => _weight;
    public Sprite IconSprite => _iconSprite;
    public Item_Info_Item Info => _info;

    [SerializeField] private int _id;
    [SerializeField] private string _name;
    [Multiline]
    [SerializeField] private string _tooltip;
    [SerializeField] private int _weight;
    [SerializeField] private Sprite _iconSprite;

    private Item_Info_Item _info;

    public ItemData(Item_Info_Item info, Sprite sprite)
    {
        _info = info;
        //_id = _info.Item_id;
        _name = _info.Item_name;
        //_tooltip = _info.Item_
        _weight = (int)_info.Item_weight;
        _iconSprite = sprite;
    }

    /// <summary> 타입에 맞는 새로운 아이템 생성 </summary>
    public abstract Item Createitem();
}
