using UnityEngine;

[CreateAssetMenu(fileName = "PotionData", menuName = "ItemData/CountableItem/PotionData")]
public class PotionItemData : CountableItemData
{
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
