using System;
using UnityEngine;

public class ItemConstructor : MonoBehaviour
{
    public static ItemData GetItemData(int index)
    {
        Item_Info_Item rawData = Item_Info.Item[index];
        ItemData itemData;

        switch (rawData.type)
        {
            case "THROW":
                goto case "DROP";

            case "DROP":

                break;

            case "WEAPON":
                itemData = new WeaponItemData(rawData, null); // 스프라이트 찾는 로직 필요

                break;

        }


        return null;
    }

    public static ItemData GetItemData(string id)
    {
        return GetItemData(Array.FindIndex(Item_Info.Item, item => item.id == id));
    }
}
