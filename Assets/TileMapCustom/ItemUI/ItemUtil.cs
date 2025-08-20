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
                return new ThrowItemData(data, null, null);
            case "DROP":
                return new ResourceItemData(data, null);
            case "HONE":
                return null;
            case "WEAPON":
                return new WeaponItemData(data, null);
            case "ARMOR":
                return new BodyArmorItemData(data, null);
            case "BACKPACK":
                return null;
            case "SUBWEAPON":
                return null;
            case "BOTTLE":
                return null;
            case "POTION":
                return new PotionItemData(data, null);
            default:
                return null;
        }
    }
}
