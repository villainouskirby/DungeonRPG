using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using EM = ExtractorMaster;

public class GuideMaker : MonoBehaviour
{
    public List<Tilemap> Floor1Ground;
    public List<Tilemap> Floor1Wall;
    public List<Tilemap> Floor2Ground;
    public List<Tilemap> Floor2Wall;
    public List<Tilemap> Floor3Ground;
    public List<Tilemap> Floor3Wall;

    public void MakeFloor(List<Tilemap> ground, List<Tilemap> wall, Vector2Int size, Vector2Int chunkSize)
    {
        bool[] tileData = new bool[size.x * size.y];
        Array.Fill(tileData, false);

        for (int i = 0; i < ground.Count; i++)
        {
            ground[i].CompressBounds();
            BoundsInt groundBounds = ground[i].cellBounds;
            Vector3Int groundStartPos = groundBounds.position;
            TileBase[] groundTiles = ground[i].GetTilesBlock(groundBounds);

            for (int y = 0; y < groundBounds.size.y; y++)
            {
                for (int x = 0; x < groundBounds.size.x; x++)
                {
                    int correctX = x + groundStartPos.x - EM.Instance.StartPos.x;
                    int correctY = y + groundStartPos.y - EM.Instance.StartPos.y;
                    TileBase tileBase = groundTiles[x + y * groundBounds.size.x];

                    Sprite sprite = null;

                    if (tileBase == null)
                    {
                        sprite = null;
                    }
                    else if (tileBase is Tile tile)
                    {
                        sprite = tile.sprite;
                    }
                    else if (tileBase is RuleTile ruleTile)
                    {
                        Vector3Int tilePos = new(correctX, correctY, 0);
                        TileData tileDataOut = new();
                        ruleTile.GetTileData(tilePos, ground[i], ref tileDataOut);
                        sprite = tileDataOut.sprite;
                    }

                    if (sprite != null)
                        tileData[correctX + correctY * size.x] = true;
                }
            }
        }


        for (int i = 0; i < wall.Count; i++)
        {
            wall[i].CompressBounds();
            BoundsInt wallBounds = wall[i].cellBounds;
            Vector3Int wallStartPos = wallBounds.position;
            TileBase[] wallTiles = wall[i].GetTilesBlock(wallBounds);

            for (int y = 0; y < wallBounds.size.y; y++)
            {
                for (int x = 0; x < wallBounds.size.x; x++)
                {
                    int correctX = x + wallStartPos.x - EM.Instance.StartPos.x;
                    int correctY = y + wallStartPos.y - EM.Instance.StartPos.y;
                    TileBase tileBase = wallTiles[x + y * wallBounds.size.x];

                    Sprite sprite = null;

                    if (tileBase == null)
                    {
                        sprite = null;
                    }
                    else if (tileBase is Tile tile)
                    {
                        sprite = tile.sprite;
                    }
                    else if (tileBase is RuleTile ruleTile)
                    {
                        Vector3Int tilePos = new(correctX, correctY, 0);
                        TileData tileDataOut = new();
                        ruleTile.GetTileData(tilePos, wall[i], ref tileDataOut);
                        sprite = tileDataOut.sprite;
                    }

                    if (sprite != null)
                        tileData[correctX + correctY * size.x] = false;
                }
            }
        }
    }
}
