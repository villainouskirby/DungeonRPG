using UnityEngine;

[CreateAssetMenu(fileName = "TileMapData", menuName = "TileMap/TileMapData")]
public class TileMapData : ScriptableObject
{
    public int Width;
    public int Height;
    public int[] Tile; // 인스펙터에서 배열 지원을 위해 1D 배열 사용
    public Vector2 PlayerSpawnPos;


    public void SetTile(int x, int y, int value)
    {
        Tile[y * Width + x] = value;
    }

    public int GetTile(int x, int y)
    {
        return Tile[y * Width + x];
    }

    public int[] GetColumnData()
    {
        int[] columnData = new int[Tile.Length];

        for(int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                columnData[x * Height + y] = Tile[y * Width + x];
            }
        }

        return columnData;
    }

    // 2D 배열을 설정하는 함수
    public void SetTileData(int[,] tileArray)
    {
        Width = tileArray.GetLength(0);
        Height = tileArray.GetLength(1);
        Tile = new int[Width * Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Tile[x + y * Width] = tileArray[x, y];
            }
        }
    }

    // 2D 배열로 변환하는 함수
    public int[,] GetTileData()
    {
        int[,] tileArray = new int[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                tileArray[x, y] = Tile[x + y * Width];
            }
        }
        return tileArray;
    }
}
