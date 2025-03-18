using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileMapOption : ITileMap
{
    public void OnOption();
    public void OffOption();
    public TileMapOptionEnum OptionType {  get; }
}
