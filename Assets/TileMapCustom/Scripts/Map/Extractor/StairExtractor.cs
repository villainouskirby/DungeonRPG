#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StairExtractor : MonoBehaviour, IExtractor
{
    public void Extract(MapEnum mapType, TileMapData mapData)
    {
        Obj2Stair(mapData);
    }

    public void Obj2Stair(TileMapData mapData)
    {
        mapData.All.StairData = new StairData[transform.childCount];

        int count = 0;
        foreach (Transform child in transform)
        {
            StairData data = new();
            Stair stair = child.GetComponent<Stair>();
            data.Entry1Height = stair.Entry1Height;
            data.Entry2Height = stair.Entry2Height;
            if (stair.StairType == StairType.LeftRight) // 왼쪽에서 오른쪽으로 갈 때 기준
            {
                Vector2 start = ExtractorMaster.Instance.CorrectPos(stair.StairOutLine1.points[0]);
                Vector2 end = ExtractorMaster.Instance.CorrectPos(stair.StairOutLine1.points[1]);
                float checkDiv = (end.y - start.y);
                if (checkDiv == 0) checkDiv = 0.1f;
                data.MoveCorrect = new((end.y - start.y)/(end.x - start.x), 0);
            }
            else
            {
                data.MoveCorrect = Vector2.zero;
            }

            data.Pos = child.transform.position;
            data.Type = (int)stair.StairType;
            data.StairEntry1 = new BoxData { Pos = stair.StairEntry1.transform.position, Offset = ExtractorMaster.Instance.CorrectPos(stair.StairEntry1.offset), Size = stair.StairEntry1.size };
            data.StairEntry2 = new BoxData { Pos = stair.StairEntry2.transform.position, Offset = ExtractorMaster.Instance.CorrectPos(stair.StairEntry2.offset), Size = stair.StairEntry2.size };
            data.StairOutLine1 = new EdgeData { Pos = stair.StairOutLine1.transform.position, Points = stair.StairOutLine1.points.Select(s => ExtractorMaster.Instance.CorrectPos(s)).ToArray() };
            data.StairOutLine2 = new EdgeData { Pos = stair.StairOutLine2.transform.position, Points = stair.StairOutLine2.points.Select(s => ExtractorMaster.Instance.CorrectPos(s)).ToArray() };
            int pc = stair.StairIn.pathCount;
            data.StairIn = new PolygonData { Pos = stair.StairIn.transform.position, Paths = new Vector2[pc][] };
            for (int i = 0; i < pc; i++)
                data.StairIn.Paths[i] = stair.StairIn.GetPath(i).Select(s => ExtractorMaster.Instance.CorrectPos(s)).ToArray();

            mapData.All.StairData[count] = data;
            count++;
        }
    }
}
#endif