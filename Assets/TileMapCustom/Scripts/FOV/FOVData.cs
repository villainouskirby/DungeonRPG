using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FOVData", menuName = "TileMap/FOVData")]
public class FOVData : ScriptableObject
{
    public FOVTile[] FOV;
    public int[] Circle;
    public int Diameter;

    public bool IsInCircle(Vector2Int relativePos)
    {
        return Circle[relativePos.y * Diameter +  relativePos.x] == 1;
    }

    public List<int> GetRelativeTile(Vector2Int pos)
    {
        return FOV[pos.y * Diameter + pos.x].relativeTile;
    }

    public void SetTileData(int[,] circleArray)
    {
        Diameter = circleArray.GetLength(0);
        Circle = new int[Diameter * Diameter];

        for (int y = 0; y < Diameter; y++)
        {
            for (int x = 0; x < Diameter; x++)
            {
                Circle[x + y * Diameter] = circleArray[x, y];
            }
        }
    }
}

[System.Serializable]
public class FOVTile
{
    public List<int> relativeTile;

    public FOVTile(List<int> relativeTile)
    {
        this.relativeTile = relativeTile;
    }

    public FOVTile()
    {
        relativeTile = new();
    }
}