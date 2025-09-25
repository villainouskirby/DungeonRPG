using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemUtilTester : MonoBehaviour
{
    public TMP_Dropdown Dropdown;

    private void Awake()
    {
        Dropdown.options.Clear();
        List<TMP_Dropdown.OptionData> option = new();
        for (int i = 0; i < Item_Info.Item.Length; i++)
        {
            option.Add(new(Item_Info.Item[i].id));
        }
        Dropdown.AddOptions(option);
    }

    public void AddItem()
    {
        var dic = SheetDataUtil.DicByKey(Item_Info.Item, x => x.id);
        ItemData itemData = ItemUtil.MakeItemData(dic[Dropdown.options[Dropdown.value].text]);
        UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(itemData);
    }
}
