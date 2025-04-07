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
        InteractionObjData interactionObjData = new();
        interactionObjData.Interaction = ExtractTilemapToInteractionObj();
        mapData.All.InteractionObjData = interactionObjData;
    }

    private List<InteractionObj> ExtractTilemapToInteractionObj()
    {
        List<InteractionObj> data = new();
        var childs = Tilemap.transform.GetComponentsInChildren<Transform>();
        
        foreach(var child in childs)
        {
            if (child.name == Tilemap.name)
                continue;

            InteractionTile interactionTile = child.GetComponent<InteractionTile>();

            switch(interactionTile.Type)
            {
                case InteractionEnum.Entry:
                    IT_EntryObj interactionObj = new(interactionTile);
                    IT_EntryTile entryTile = (IT_EntryTile)interactionTile;
                    interactionObj.MapType = entryTile.MapType;
                    data.Add(interactionObj);
                    break;
                case InteractionEnum.Npc:
                    IT_NpcObj npcObj = new(interactionTile);
                    IT_NpcTile npcTile = (IT_NpcTile)interactionTile;
                    npcObj.NpcName = npcTile.NpcName;
                    data.Add(npcObj);
                    break;
            }
        }

        return data;
    }
}
