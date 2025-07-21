using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DL = DataLoader;

public class CameraAreaManager : MonoBehaviour, ITileMapOption
{
    public GameObject[] AllActiveCameraArea;
    public GameObject CameraAreaPrefab;

    public TileMapOptionEnum OptionType => TileMapOptionEnum.CameraAreaManager;

    public void Init()
    {
    }

    public void InitMap(MapEnum mapType)
    {
        DeleteAll();
        GenerateAllCameraArea();
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

    public int Prime => (int)TileMapOptionPrimeEnum.CameraAreaManager;

    private void GenerateAllCameraArea()
    {
        AllActiveCameraArea = new GameObject[DL.Instance.All.CameraAreaData.Length];
        for(int i = 0; i < DL.Instance.All.CameraAreaData.Length; i++)
        {
            GameObject target = Instantiate(CameraAreaPrefab, transform);
            CameraArea cameraArea = target.GetComponent<CameraArea>();
            cameraArea.Set(DL.Instance.All.CameraAreaData[i]);
            AllActiveCameraArea[i] = target;
        }
    }

    private void DeleteAll()
    {
        for(int i = 0;i < AllActiveCameraArea.Length; i++)
        {
            Destroy(AllActiveCameraArea[i]);
        }
    }
}
