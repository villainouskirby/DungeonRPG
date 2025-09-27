using DG.Tweening.Plugins.Core.PathCore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TM = TileMapMaster;

public class HeightManager : MonoBehaviour, ITileMapBase
{
    public int PlayerHeight;
    public int CurrentLayer = 0;

    public int Prime => (int)TileMapBasePrimeEnum.HeightManager;

    public void Init()
    {
    }

    public void InitMap(MapEnum mapType)
    {
    }

    public void StartMap(MapEnum mapType)
    {
        PlayerMoveChecker.Instance.AddMoveAction(CheckPlayerHeight);
    }

    public void CheckPlayerHeight(Vector2Int playerPos)
    {
        PlayerHeight = ChunkManager.Instance.GetHeight(playerPos, CurrentLayer);
        Shader.SetGlobalFloat("_PlayerHeight", PlayerHeight);
    }
}
