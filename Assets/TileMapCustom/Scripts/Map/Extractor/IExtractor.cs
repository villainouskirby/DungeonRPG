using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExtractor
{
    public void Extract(MapEnum mapType, TileMapData mapData);
}

public interface IExtractorLate
{
    public void Extract(MapEnum mapType, TileMapData mapData);
}