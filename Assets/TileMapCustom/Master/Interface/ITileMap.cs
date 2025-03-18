using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileMap : IPrime
{
    public void Init();
    public void InitMap(MapEnum mapType);
    public void StartMap(MapEnum mapType);
}
