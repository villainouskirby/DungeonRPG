using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteractionObjExtractor : MonoBehaviour, IExtractor
{
    public Tilemap          Tilemap;

    public void Extract(MapEnum mapEnum, TileMapData mapData)
    {
        mapData.All.InteractionObjData = ExtractTilemap2InteractionObj();
    }

    private InteractionObjData ExtractTilemap2InteractionObj()
    {
        InteractionObjData result = new();
        var childs = Tilemap.transform.GetComponentsInChildren<Transform>();
        
        foreach(var child in childs)
        {
            if (child.name == Tilemap.name)
                continue;

            InteractionTile interactionTile = child.GetComponent<InteractionTile>();
            if (interactionTile == null) continue;
            interactionTile.Z = child.transform.position.z;
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            interactionTile.LayerName = sr.sortingLayerName;
            interactionTile.LayerIndex = sr.sortingOrder;

            switch(interactionTile.Type)
            {
                case InteractionEnum.Entry:
                    IT_EntryObj entryObj = new(interactionTile);
                    IT_EntryTile entryTile = (IT_EntryTile)interactionTile;
                    entryObj.MapType = entryTile.MapType;

                    result.Entry ??= new();
                    result.Entry.Add(entryObj);
                    break;
                case InteractionEnum.Npc:
                    IT_NpcObj npcObj = new(interactionTile);
                    IT_NpcTile npcTile = (IT_NpcTile)interactionTile;
                    npcObj.NpcName = npcTile.NpcName;

                    result.Npc ??= new();
                    result.Npc.Add(npcObj);
                    break;
                case InteractionEnum.Teleport:
                    IT_TeleportObj teleportObj = new(interactionTile);
                    IT_TeleportTile teleportTile = (IT_TeleportTile)interactionTile;
                    teleportObj.TargetPos = teleportTile.TargetPos;

                    result.Teleport ??= new();
                    result.Teleport.Add(teleportObj);
                    break;
            }
        }

        return result;
    }
}
