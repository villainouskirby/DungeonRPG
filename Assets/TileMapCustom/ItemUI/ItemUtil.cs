using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemUtil
{
    public static ItemData MakeItemData(Item_Info_Item data)
    {
        switch (data.type)
        {
            case "THROW":
                return new ThrowItemData(data, null);
            case "DROP":
                return new ResourceItemData(data);
            case "HONE":
                return null;
            case "WEAPON":
                return new WeaponItemData(data);
            case "ARMOR":
                return new ArmorItemData(data);
            case "BACKPACK":
                return null;
            case "SUBWEAPON":
                return null;
            case "BOTTLE":
                return null;
            case "POTION":
                return new PotionItemData(data);
            default:
                return null;
        }
    }
}
