using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantFarm : FarmableBase
{
    private FarmEnum _farm = FarmEnum.Plant;
    private PlantEnum _plant;

    public override void Init()
    {
        base.Init();
        _plant = (PlantEnum)Enum.Parse(typeof(PlantEnum), name.Split("(")[0]);
        _type = _farm;
        _plantType = _plant;
    }
}
