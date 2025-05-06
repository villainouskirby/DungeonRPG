using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoManager : MonoBehaviour, ITileMapBase
{
    public List<GameObject> ActiveDecoObj;

    public GameObject DecoPrefab_default;
    public GameObject DecoPrefab_circle;
    public GameObject DecoPrefab_box;
    public GameObject DecoPrefab_poly;

    public void Init()
    {

    }

    public void InitMap(MapEnum mapType)
    {

    }

    public void StartMap(MapEnum mapType)
    {

    }

    public int Prime { get { return (int)TileMapBasePrimeEnum.DecoManager; } }
}
