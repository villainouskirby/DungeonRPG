using System.Collections.Generic;
using UnityEngine;

public class IngredientItemData : CountableItemData
{
    public override Item Createitem()
    {
        return new IngredientItem(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
