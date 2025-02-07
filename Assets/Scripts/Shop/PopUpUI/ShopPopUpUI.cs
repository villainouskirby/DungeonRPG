using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPopUpUI : ItemPopUpUI
{
    [SerializeField] private ShopUI _shopUI;

    public void OpenInfo(int index)
    {
        ItemData data= _shopUI.GetItemData(index).Data;
        if (data == null)
        {
            CloseInfo();
            return;
        }

        base.OpenInfo(data);
    }
}
