using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemData : CountableItemData
{
    // ITM_* 시트의 "아이템 상세정보 DT" (예: PAR_MIN_ROC)
    public string PAR_DT { get; private set; }

    public Item_Info_ThrowableItem Info { get; private set; }

    public ThrowItemData(Item_Info_Item info, string parDt) : base(info)
    {
        PAR_DT = parDt;
    }
    public void SetInfo(Item_Info_ThrowableItem row) => Info = row;
    public override Item Createitem() => new ThrowItem(this, 1);
}