#if UNITY_EDITOR
using EM = ExtractorMaster;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadowHelper : MonoBehaviour
{
    public bool Active;

    [Header("Target TileMaps")]
    public List<Tilemap> Target = new();
    public ExtractorMaster EM;

    private List<Vector2Int> _shadowTile = new();

    private void OnDrawGizmos()
    {
        if (!Active)
            return;

        _shadowTile.Clear();

        if (!EM.IndividualShadow)
        {
            for (int i = 0; i < Target.Count; i++)
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

                        if (EM.ShadowType.Sprites.Contains(sprite))
                        {
                            _shadowTile.Add(new(correctX, correctY));
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < Target.Count; i++)
            {
                if (EM.ShadowSettings.Count < i + 1)
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

                        if (EM.ShadowType.Sprites.Contains(sprite))
                            _shadowTile.Add(new(correctX, correctY));
                    }
                }
            }
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < _shadowTile.Count; i++)
        {
            Gizmos.DrawWireCube(new(_shadowTile[i].x + 0.5f, _shadowTile[i].y + 0.5f, 0), new(1, 1, 0));
        }
    }
}
#endif