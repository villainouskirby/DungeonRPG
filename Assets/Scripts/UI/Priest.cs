using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Priest : NPCBase<Priest>
{
    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
        _npcName = "priest";
    }
}
