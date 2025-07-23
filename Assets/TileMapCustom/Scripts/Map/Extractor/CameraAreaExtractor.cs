using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAreaExtractor : MonoBehaviour, IExtractor
{
    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        Obj2CameraArea(mapData);
    }

    public void Obj2CameraArea(TileMapData mapData)
    {
        mapData.All.CameraAreaData = new CameraAreaData[transform.childCount];

        int count = 0;
        foreach(Transform child in transform)
        {
            CameraAreaData data = new();
            CameraArea cameraArea = child.GetComponent<CameraArea>();
            BoxCollider2D collider = child.GetComponent<BoxCollider2D>();
            GameObject fixPosObj = child.GetChild(0).gameObject;

            data.Pos = child.position;
            data.Pos = ExtractorMaster.Instance.CorrectPos(data.Pos);
            data.CustomZ = child.position.z;
            data.Size = collider.size;
            data.FixPos = fixPosObj.transform.position;
            data.IsFixed = cameraArea.Data.IsFixed;
            data.IsCustomZ = cameraArea.Data.IsCustomZ;

            mapData.All.CameraAreaData[count] = data;

            count++;
        }
    }
}
