using UnityEngine;

public class PotionItemData : CountableItemData
{
    public float Percentage => percentage;
    public float Duration => duration;
    public float Healamount => healamount;

    [SerializeField] private float percentage;

    [SerializeField] private float duration;

    [SerializeField] private float healamount;

    public PotionItemData(Item_Info_Item info, Sprite sprite) : base(info, sprite)
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override Item Createitem()
    {
        return new PotionItem(this);
    }
}
