using DG.Tweening.Plugins.Core.PathCore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TM = TileMapMaster;

public class HeightManager : MonoBehaviour, ITileMapBase
{
    public static HeightManager Instance { get { return _instance; } }
    private static HeightManager _instance;

    public float PlayerHeight;
    public int CurrentLayer = 0;
    public int GroundLayer = 0;
    public bool AutoHeight = true;

    public int Prime => (int)TileMapBasePrimeEnum.HeightManager;

    public void Init()
    {
        _instance = this;
    }

    public void InitMap(MapEnum mapType)
    {
        HeightManager.Instance.GroundLayer = DataLoader.Instance.All.MapStartGroundLayer;
        HeightManager.Instance.CurrentLayer = DataLoader.Instance.All.MapStartLayer;
    }

    public void StartMap(MapEnum mapType)
    {
        PlayerMoveChecker.Instance.AddMoveAction(CheckPlayerHeight);
    }

    public void CheckPlayerHeight(Vector2Int playerPos)
    {
        if (AutoHeight)
        {
            PlayerHeight = ChunkManager.Instance.GetHeight(playerPos, GroundLayer);
            Shader.SetGlobalFloat("_PlayerHeight", PlayerHeight);
        }
    }

    public void SetPlayerHeight(float height)
    {
        PlayerHeight = height;
        Shader.SetGlobalFloat("_PlayerHeight", PlayerHeight);
    }

    public void ChangeLayer(int layer)
    {
        CurrentLayer = layer;
        ChunkManager.Instance.RefreshLayer(layer);
    }
}
