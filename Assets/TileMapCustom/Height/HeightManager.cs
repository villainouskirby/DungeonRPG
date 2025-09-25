using DG.Tweening.Plugins.Core.PathCore;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TM = TileMapMaster;

public class HeightManager
{
    public Stream Stream;

    public void SetStream(MapEnum mapType)
    {
        string path = JJSave.GetSavePath($"{mapType.ToString()}_Height", $"JJSave/SaveFile/{TM.Instance.SaveSlotIndex}/{mapType.ToString()}/");

        Stream?.Close();
        Stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.ReadWrite
        );
    }
}
