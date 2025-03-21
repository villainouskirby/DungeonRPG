using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralFarm : FarmableBase
{
    private FarmEnum _farm = FarmEnum.Mineral;
    private MineralEnum _mineral;

    public override void Init()
    {
        base.Init();
        _mineral = (MineralEnum)Enum.Parse(typeof(MineralEnum), name.Split("(")[0]);
        _type = _farm;
        _mineralType = _mineral;
    }
}
