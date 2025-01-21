using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public int ID => _id;
    public string Name => _name;
    public string Tooltip => _tooltip;
    public int Weight => _weight;
    public Sprite IconSprite => _iconSprite;

    [SerializeField] private int _id;
    [SerializeField] private string _name;
    [Multiline]
    [SerializeField] private string _tooltip;
    [SerializeField] private int _weight;
    [SerializeField] private Sprite _iconSprite;

    /// <summary> 타입에 맞는 새로운 아이템 생성 </summary>
    public abstract Item Createitem();
}
