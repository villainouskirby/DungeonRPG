using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DL = DataLoader;

public class EventAreaManager : MonoBehaviour, ITileMapOption
{
    public GameObject[] AllActiveEventArea;
    public GameObject EventAreaPrefab;
    public TileMapOptionEnum OptionType => TileMapOptionEnum.EventAreaManager;

    public void Init()
    {
    }

    public void InitMap(MapEnum mapType)
    {
        DeleteAll();
        GenerateAllEventArea();
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

    public int Prime => (int)TileMapOptionPrimeEnum.EventAreaManager;

    private void GenerateAllEventArea()
    {
        AllActiveEventArea = new GameObject[DL.Instance.All.EventAreaData.Length];
        for (int i = 0; i < DL.Instance.All.EventAreaData.Length; i++)
        {
            GameObject target = Instantiate(EventAreaPrefab, transform);
            EventArea eventArea = target.GetComponent<EventArea>();
            eventArea.Set(DL.Instance.All.EventAreaData[i]);
            AllActiveEventArea[i] = target;
        }
    }

    private void DeleteAll()
    {
        for (int i = 0; i < AllActiveEventArea.Length; i++)
        {
            Destroy(AllActiveEventArea[i]);
        }
    }
}
