using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DL = DataLoader;

public class StairManager : MonoBehaviour, ITileMapOption
{
    public GameObject[] AllActiveStair;
    public GameObject StairPrefab;

    public TileMapOptionEnum OptionType => TileMapOptionEnum.StairManager;

    public void Init()
    {
    }

    public void InitMap(MapEnum mapType)
    {
        DeleteAll();
        GenerateAllStair();
    }

    public void OffOption()
    {
        DeleteAll();
    }

    public void OnOption()
    {
    }

    public void StartMap(MapEnum mapType)
    {
    }

    public int Prime => (int)TileMapOptionPrimeEnum.StairManager;

    private void GenerateAllStair()
    {
        AllActiveStair = new GameObject[DL.Instance.All.StairData.Length];
        for (int i = 0; i < DL.Instance.All.StairData.Length; i++)
        {
            GameObject target = Instantiate(StairPrefab, transform);
            Stair stair = target.GetComponent<Stair>();
            stair.Set(DL.Instance.All.StairData[i]);
            AllActiveStair[i] = target;
        }
    }

    private void DeleteAll()
    {
        for (int i = 0; i < AllActiveStair.Length; i++)
        {
            Destroy(AllActiveStair[i]);
        }
    }
}
