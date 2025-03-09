using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InteractionObjExtractor : MonoBehaviour
{
    static public string    DataFilePath = "Assets/Resources/";
    static public string    DataFileDirectory = "TileMapData/";


    public Tilemap          Tilemap;

    public MapEnum         MapType;
    void Start()
    {
        SaveInteractionObjData();
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
                    interactionObj.TargetSceneName = entryTile.TargetSceneName;
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

    private void SaveInteractionObjData()
    {
        string assetName = MapType.ToString();
        string directoryPath = $"{DataFilePath}{DataFileDirectory}{assetName}/";

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);



        InteractionObjData interactionObjData = ScriptableObject.CreateInstance<InteractionObjData>();
        interactionObjData.Interaction = ExtractTilemapToInteractionObj();

        string interactionObjDataName = $"{assetName}InteractionObjData";

        // ScriptableObject를 에셋으로 저장
        AssetDatabase.DeleteAsset($"{directoryPath}{interactionObjDataName}.asset");
        AssetDatabase.CreateAsset(interactionObjData, $"{directoryPath}{interactionObjDataName}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"InteractionObjData asset created at: {directoryPath}");
    }
}
