using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionObj
{
    public InteractionEnum Type;
    public Vector2 TilePos;

    public InteractionObj(InteractionTile interactionTile)
    {
        int tileX = Mathf.FloorToInt((interactionTile.transform.position.x + 9) / 1f);
        int tileY = Mathf.FloorToInt((interactionTile.transform.position.y + 5) / 1f);
        TilePos = new(tileX, tileY);
        Type = interactionTile.Type;
    }
}
