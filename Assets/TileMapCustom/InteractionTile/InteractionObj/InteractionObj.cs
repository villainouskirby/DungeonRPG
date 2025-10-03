using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionObj
{
    public InteractionEnum Type;
    public Vector2 TilePos;
    public float Z;
    public string LayerName;
    public int LayerIndex;

    public InteractionObj(InteractionTile interactionTile)
    {
        int tileX = Mathf.FloorToInt((interactionTile.transform.position.x) / 1f);
        int tileY = Mathf.FloorToInt((interactionTile.transform.position.y) / 1f);
        TilePos = new(tileX, tileY);
#if UNITY_EDITOR
        TilePos = ExtractorMaster.Instance.CorrectPos(TilePos);
#endif
        Type = interactionTile.Type;
        Z = interactionTile.Z;
        LayerName = interactionTile.LayerName;
        LayerIndex = interactionTile.LayerIndex;
    }

    public InteractionObj()
    {

    }
}
