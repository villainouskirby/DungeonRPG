using UnityEngine;

[CreateAssetMenu(fileName = "Test", menuName = "Tilemap/Tilemap Data")]
public class TilemapDataTest : ScriptableObject
{
    public int width;
    public int height;
    public int[] tiles; // 인스펙터에서 배열 지원을 위해 1D 배열 사용

    // 2D 배열을 설정하는 함수
    public void SetTileData(int[,] tileArray)
    {
        width = tileArray.GetLength(0);
        height = tileArray.GetLength(1);
        tiles = new int[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tiles[x + y * width] = tileArray[x, y];
            }
        }
    }

    // 2D 배열로 변환하는 함수
    public int[,] GetTileData()
    {
        int[,] tileArray = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tileArray[x, y] = tiles[x + y * width];
            }
        }
        return tileArray;
    }
}
