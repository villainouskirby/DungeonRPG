using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteractionObjExtractor : MonoBehaviour, IExtractor
{
    public Tilemap          Tilemap;

    public void Extract(MapEnum mapEnum, ref TileMapData mapData)
    {
        mapData.All.InteractionObjData = ExtractTilemapToInteractionObj();
    }

    private InteractionObjData ExtractTilemapToInteractionObj()
    {
        InteractionObjData result = null;
        var childs = Tilemap.transform.GetComponentsInChildren<Transform>();
        
        foreach(var child in childs)
        {
            if (child.name == Tilemap.name)
                continue;

            result = new();
            InteractionTile interactionTile = child.GetComponent<InteractionTile>();

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
            }
        }

        return result;
    }
}
