using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapMaster : MonoBehaviour
{
    public static TileMapMaster Instance { get { return _instance; } }
    private static TileMapMaster _instance;

    [Header("Map Settings")]
    public List<MapSettings> MapSettings;

    [Header("Camera Settings")]
    public Camera TargetCamera;

    [Header("Player Settings")]
    public GameObject Player;

    private List<ITileMapBase> _base;
    private List<ITileMapOption> _option;
    private Dictionary<MapEnum, MapSettings> _mapSettings;

    public TileMapController Controller;
    public TileMapController GuideController;

    public void Awake()
    {
        Init();
        StartTileMap(MapEnum.Map1);
    }

    [ContextMenu("Init MapSettings InSpector")]
    public void InitMapSettingsInSpector()
    {
        for(int i = 0; i < (int)MapEnum.End; i++)
        {
            bool exit = false;
            for (int j = 0; j < MapSettings.Count; j++)
            {
                if (MapSettings[j].MapType == (MapEnum)i)
                {
                    exit = true;
                    break;
                }
            }

            if (!exit)
                MapSettings.Add(new((MapEnum)i));
        }
    }

    public void Init()
    {
        _instance = this;

        // Base, Option들을 전부 찾아서 Prime 기준으로 Sort해준다.
        CollectTarget(out _base);
        CollectTarget(out _option);
        SortByPrime();

        // 정렬이 끝났다면 정렬 기준으로 Init해준다.
        InitAll(_base);
        InitAll(_option);
        // 일련의 과정을 해주는 이유는 서순 문제를 해소하기 위함임.

        // Map Settings를 Dic에 맵핑 해준다.
        SetMapSettingDic();
    }

    public void StartTileMap(MapEnum mapType)
    {
        for(int i = 0; i < _base.Count; i++)
        {
            _base[i].StartMap(mapType);
        }

        for (int i = 0; i < _option.Count; i++)
        {
            ITileMapOption option = _option[i];
            option.OffOption();
        }

        if (mapType == MapEnum.None)
            gameObject.SetActive(false);

        for (int i = 0; i < _option.Count; i++)
        {
            ITileMapOption option = _option[i];
            if (_mapSettings[mapType].IsOptionActive(option.OptionType))
            {
                option.OnOption();
                _option[i].StartMap(mapType);
            }
        }

        Controller.InitializeTileMap();
        //GuideController.InitializeTileMap();
    }


    private void SetMapSettingDic()
    {
        _mapSettings = new();
        for(int i = 0; i < MapSettings.Count; i++)
        {
            _mapSettings[MapSettings[i].MapType] = MapSettings[i];
            MapSettings[i].Init();
        }
    }

    public void CollectTarget<T>(out List<T> result)
    {
        result = new();

        foreach (var component in GetComponentsInChildren<Component>(true))
        {
            if (component is T targetInterface)
            {
                result.Add(targetInterface);
            }
        }
    }

    private void InitAll<T>(List<T> target) where T : ITileMap
    {
        for(int i = 0; i < target.Count; i++)
        {
            target[i].Init();
        }
    }

    private void SortByPrime()
    {
        _base.Sort(Compare);
        _option.Sort(Compare);
    }

    public int Compare(ITileMapBase x, ITileMapBase y)
    {
        if (x == null || y == null) return 0;
        return x.Prime.CompareTo(y.Prime);
    }

    public int Compare(ITileMapOption x, ITileMapOption y)
    {
        if (x == null || y == null) return 0;
        return x.Prime.CompareTo(y.Prime);
    }
}

[System.Serializable]
public class MapSettings
{
    public MapEnum MapType;
    public OptionActive[] OptionsActive;
    private Dictionary<TileMapOptionEnum, bool> _optionsActive;

    public void Init()
    {
        _optionsActive = new();

        for (int i = 0; i < OptionsActive.Length; i++)
        {
            _optionsActive[OptionsActive[i].OptionType] = OptionsActive[i].Active;
        }
    }

    public bool IsOptionActive(TileMapOptionEnum optionType)
    {
        return _optionsActive[optionType];
    }

    public MapSettings(MapEnum mapEnum)
    {
        MapType = mapEnum;
        OptionsActive = new OptionActive[(int)TileMapOptionEnum.End];
        for(int i = 0 ;i < (int)TileMapOptionEnum.End; i++)
        {
            OptionsActive[i] = new((TileMapOptionEnum)i, true);
        }
    }
}

[System.Serializable]
public class OptionActive
{
    public TileMapOptionEnum OptionType;
    public bool Active;

    public OptionActive(TileMapOptionEnum optionType, bool active)
    {
        OptionType = optionType;
        Active = active;
    }
}