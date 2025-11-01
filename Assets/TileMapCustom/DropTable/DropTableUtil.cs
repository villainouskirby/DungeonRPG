using DBUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResourceNode_Info;

public static class DropTableUtil
{
    private static Dictionary<string, string> _mappingTable;
    private static Dictionary<string, ResourceItemData> _mappingItem;

    static DropTableUtil()
    {
        _mappingTable = new();

        for(int i = 0; i < ResourceNode_DropTable.Length; i++)
        {
            ResourceNode_Info_ResourceNode_DropTable dropTable = ResourceNode_DropTable[i];
            _mappingTable[dropTable.DropTable_id] = dropTable.DropTable_Info;
        }

        _mappingItem = new();

        for(int i = 0; i < Item_Info.Item.Length; i++)
        {
            ResourceItemData resourceItemData = ItemDataConstructor.GetItemData(i) as ResourceItemData;
            _mappingItem[Item_Info.Item[i].id] = resourceItemData;
        }
    }


    public static (ResourceItemData data, int amount) GetDropItemFromTable(string dropTableName)
    {
        (ResourceItemData data, int amount) result = new();

        string dropTableInfo = _mappingTable[dropTableName];

        string[] itemInfos = dropTableInfo.Trim().Split("~");
        int dropPercent = Random.Range(1, 101);
        int sumPercent = 0;

        for (int i = 0; i < itemInfos.Length; i++)
        {
            string[] itemInfo = itemInfos[i].Split("/");

            string itemId = itemInfo[0];
            string percent = itemInfo[1];
            string min = itemInfo[2];
            string max = itemInfo[3];

            int amount = Random.Range(int.Parse(min), int.Parse(max) + 1);
            sumPercent += int.Parse(percent);
            bool isDrop = dropPercent <= sumPercent;
                
            if (!isDrop)
                continue;

            ResourceItemData data = _mappingItem[itemId];
            result = (data, amount);
            break;
        }

        return result;
    }
}
