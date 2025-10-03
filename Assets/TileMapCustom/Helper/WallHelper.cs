#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallHelper : MonoBehaviour
{
    public bool Active;

    [Header("Target TileMaps")]
    public List<Tilemap> Target = new();
    public ExtractorMaster EM;

    private List<Vector2Int> _wallTile = new();

    private void OnDrawGizmos()
    {
        if (!Active)
            return;

        _wallTile.Clear();

        if (!EM.IndividualWall)
        {
            for(int i = 0; i < Target.Count; i++)
            {
                if (!Target[i].gameObject.activeSelf)
                    continue;

                Target[i].CompressBounds();
                BoundsInt bounds = Target[i].cellBounds;
                Vector3Int startPos = bounds.position;

                TileBase[] tiles = Target[i].GetTilesBlock(bounds);

                for (int y = 0; y < bounds.size.y; y++)
                {
                    for (int x = 0; x < bounds.size.x; x++)
                    {
                        int correctX = x + startPos.x;
                        int correctY = y + startPos.y;

                        TileBase tileBase = tiles[x + y * bounds.size.x];

                        Sprite sprite = null;

                        if (tileBase == null)
                        {
                            continue;
                        }
                        else if (tileBase is Tile tile)
                        {
                            sprite = tile.sprite;
                        }
                        else if (tileBase is RuleTile ruleTile)
                        {
                            Vector3Int tilePos = new(correctX, correctY, 0);
                            TileData tileDataOut = new();
                            ruleTile.GetTileData(tilePos, Target[i], ref tileDataOut);
                            sprite = tileDataOut.sprite;
                        }

                        if (EM.WallType.Sprites.Contains(sprite))
                        {
                            _wallTile.Add(new(correctX, correctY));
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < Target.Count; i++)
            {
                if (EM.WallSettings.Count < i + 1)
                    continue;
                if (!Target[i].gameObject.activeSelf)
                    continue;

                Target[i].CompressBounds();
                BoundsInt bounds = Target[i].cellBounds;
                Vector3Int startPos = bounds.position;

                TileBase[] tiles = Target[i].GetTilesBlock(bounds);

                for (int y = 0; y < bounds.size.y; y++)
                {
                    for (int x = 0; x < bounds.size.x; x++)
                    {
                        int correctX = x + startPos.x;
                        int correctY = y + startPos.y;

                        TileBase tileBase = tiles[x + y * bounds.size.x];

                        Sprite sprite = null;

                        if (tileBase == null)
                        {
                            continue;
                        }
                        else if (tileBase is Tile tile)
                        {
                            sprite = tile.sprite;
                        }
                        else if (tileBase is RuleTile ruleTile)
                        {
                            Vector3Int tilePos = new(correctX, correctY, 0);
                            TileData tileDataOut = new();
                            ruleTile.GetTileData(tilePos, Target[i], ref tileDataOut);
                            sprite = tileDataOut.sprite;
                        }

                        if (EM.WallSettings[i].Sprites.Contains(sprite))
                            _wallTile.Add(new(correctX, correctY));
                    }
                }
            }
        }

        Gizmos.color = Color.green;
        for(int i = 0; i < _wallTile.Count; i++)
        {
            Gizmos.DrawWireCube(new(_wallTile[i].x +0.5f, _wallTile[i].y + 0.5f, 0), new(1, 1, 0));
        }
    }
}
#endif