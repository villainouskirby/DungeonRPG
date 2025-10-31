using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Stair : MonoBehaviour
{
    public BoxCollider2D StairEntry1;
    public BoxCollider2D StairEntry2;
    public EdgeCollider2D StairOutLine1;
    public EdgeCollider2D StairOutLine2;
    public PolygonCollider2D StairIn;
    public bool IsEntry = false;
    public bool IsIn = false;
    public StairType StairType = StairType.LeftRight;
    public Vector2 MoveCorrect = Vector2.zero; // x, y ->  x : x가 1 움직일때 y가 움직이는 양 y : y가 1 움직일때 x가 움직이는 양

    [Header("Stair Settings")]
    public int Entry1Height;
    public int Entry2Height;
    public float Speed = 0.9f;

    public void Set(StairData data)
    {
        transform.position = data.Pos;
        StairType = (StairType)data.Type;
        MoveCorrect = data.MoveCorrect;
        Speed = data.Speed;
        Entry1Height = data.Entry1Height;
        Entry2Height = data.Entry2Height;

        StairEntry1.transform.position = data.StairEntry1.Pos;
        StairEntry1.offset = data.StairEntry1.Offset;
        StairEntry1.size = data.StairEntry1.Size;
        StairEntry2.transform.position = data.StairEntry2.Pos;
        StairEntry2.offset = data.StairEntry2.Offset;
        StairEntry2.size = data.StairEntry2.Size;
        StairOutLine1.transform.position = data.StairOutLine1.Pos;
        StairOutLine1.points = data.StairOutLine1.Points;
        StairOutLine2.transform.position = data.StairOutLine2.Pos;
        StairOutLine2.points = data.StairOutLine2.Points;
        StairIn.transform.position = data.StairIn.Pos;
        StairIn.pathCount = data.StairIn.Paths.Length;
        for (int i = 0; i < data.StairIn.Paths.Length; i++)
            StairIn.SetPath(i, data.StairIn.Paths[i]);

        _startY = Mathf.Min(StairOutLine1.points[0].y, StairOutLine1.points[1].y);
        _startY += StairOutLine1.gameObject.transform.position.y;
        _endY = Mathf.Max(StairOutLine1.points[0].y, StairOutLine1.points[1].y);
        _endY += StairOutLine1.gameObject.transform.position.y;
        _startX = Mathf.Min(StairOutLine1.points[0].x, StairOutLine1.points[1].x);
        _startX += StairOutLine1.gameObject.transform.position.x;
        _endX = Mathf.Max(StairOutLine1.points[0].x, StairOutLine1.points[1].x);
        _endX += StairOutLine1.gameObject.transform.position.x;
    }

    private float _startY;
    private float _endY;
    private float _startX;
    private float _endX;

    private void Update()
    {
        if (IsIn)
        {
            float process = 0;
            switch (StairType)
            {
                case StairType.LeftRight:
                    process = (TileMapMaster.Instance.Player.transform.position.x - _startX) / (_endX - _startX);
                    break;
                case StairType.UpDown:
                    process = (TileMapMaster.Instance.Player.transform.position.y - _startY) / (_endY - _startY);
                    break;
            }
            HeightManager.Instance.SetPlayerHeight(Mathf.Lerp(Entry1Height, Entry2Height, process));
        }
    }
}

public enum StairType
{
    LeftRight,
    UpDown,
}

[System.Serializable]
public class BoxData
{
    public Vector2 Pos;
    public Vector2 Offset;
    public Vector2 Size;
}

[System.Serializable]
public class EdgeData
{
    public Vector2 Pos;
    public Vector2[] Points;
}

[System.Serializable]
public class PolygonData
{
    public Vector2 Pos;
    public Vector2[][] Paths;
}

[System.Serializable]
public class StairData
{
    public Vector2 Pos;
    public BoxData StairEntry1;
    public BoxData StairEntry2;
    public EdgeData StairOutLine1;
    public EdgeData StairOutLine2;
    public PolygonData StairIn;
    public int Entry1Height = 0;
    public int Entry2Height = 1;
    public float Speed = 0.9f;
    public Vector2 MoveCorrect;
    public int Type;
}