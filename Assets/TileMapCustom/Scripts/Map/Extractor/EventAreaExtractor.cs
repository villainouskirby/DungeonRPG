using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAreaExtractor : MonoBehaviour, IExtractor
{
    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        Obj2EventArea(mapData);
    }

    public void Obj2EventArea(TileMapData mapData)
    {
        mapData.All.EventAreaData = new EventAreaData[transform.childCount];

        int count = 0;
        foreach (Transform child in transform)
        {
            EventAreaData data = new();
            EventArea eventArea = child.GetComponent<EventArea>();
            BoxCollider2D collider = child.GetComponent<BoxCollider2D>();

            data = eventArea.Data;
            data.Pos = child.position;
            data.Pos = ExtractorMaster.Instance.CorrectPos(data.Pos);
            data.Size = collider.size;

            mapData.All.EventAreaData[count] = data;

            count++;
        }
    }
}
