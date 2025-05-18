using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TM = TileMapMaster;
using DL = DataLoader;

public class InteractionObjManager : MonoBehaviour, ITileMapOption
{
    public static string DataFilePath = "Interaction/Prefabs/";

    private GameObject _root;

    public List<GameObject> AllInteraction;

    private bool _isActive = false;

    public int Prime { get { return (int)TileMapOptionPrimeEnum.InteractionObjManager; } }

    public void Init()
    {
        _root = GameObject.Find("@InteractionObj");
    }

    public void InitMap(MapEnum mapType)
    {
        ResetInteractionObj();

        if (DL.Instance.All.InteractionObjData.Npc != null)
        {
            for (int i = 0; i < DL.Instance.All.InteractionObjData.Npc.Count; i++)
            {
                GenerateInteractionObj(DL.Instance.All.InteractionObjData.Npc[i]);
            }
        }
        if (DL.Instance.All.InteractionObjData.Entry != null)
        {
            for (int i = 0; i < DL.Instance.All.InteractionObjData.Entry.Count; i++)
            {
                GenerateInteractionObj(DL.Instance.All.InteractionObjData.Entry[i]);
            }
        }
        if (DL.Instance.All.InteractionObjData.Teleport != null)
        {
            for (int i = 0; i < DL.Instance.All.InteractionObjData.Teleport.Count; i++)
            {
                GenerateInteractionObj(DL.Instance.All.InteractionObjData.Teleport[i]);
            }
        }
    }

    public void StartMap(MapEnum mapType)
    {
        InitMap(mapType);
    }

    public void OnOption()
    {
        if (_isActive)
            return;

        _isActive = true;
    }

    public void OffOption()
    {
        if (!_isActive)
            return;

        ResetInteractionObj();
        _isActive = false;
    }

    private void ResetInteractionObj()
    {
        for(int i = 0; i < AllInteraction.Count; i++)
        {
            Destroy(AllInteraction[i]);
        }
    }

    public TileMapOptionEnum OptionType { get { return TileMapOptionEnum.InteractionObjManager; } }


    private void GenerateInteractionObj(InteractionObj interactionObj)
    {
        switch (interactionObj.Type)
        {
            case InteractionEnum.Entry:
                GameObject interaction = Instantiate(
                    Resources.Load<GameObject>($"{DataFilePath}IT_{interactionObj.Type.ToString()}")
                );

                interaction.transform.parent = _root.transform;
                interaction.transform.position = new((interactionObj.TilePos.x + 0.5f) * MapManager.Instance.TileSize, (interactionObj.TilePos.y + 0.5f) * MapManager.Instance.TileSize, interactionObj.Z);
                interaction.GetComponent<IT_EntryFunc>().Init((IT_EntryObj)interactionObj);
                AllInteraction.Add(interaction);
                break;

            case InteractionEnum.Teleport:
                GameObject teleport = Instantiate(
                    Resources.Load<GameObject>($"{DataFilePath}IT_{interactionObj.Type.ToString()}")
                );

                teleport.transform.parent = _root.transform;
                teleport.transform.position = new((interactionObj.TilePos.x + 0.5f) * MapManager.Instance.TileSize, (interactionObj.TilePos.y + 0.5f) * MapManager.Instance.TileSize, interactionObj.Z);
                teleport.GetComponent<IT_TeleportFunc>().Init((IT_TeleportObj)interactionObj);
                AllInteraction.Add(teleport);
                break;

            case InteractionEnum.Npc:
                GameObject npc = Instantiate(
                    Resources.Load<GameObject>($"{DataFilePath}IT_{interactionObj.Type.ToString()}")
                );

                npc.transform.parent = _root.transform;
                npc.transform.position = new((interactionObj.TilePos.x + 0.5f) * MapManager.Instance.TileSize, (interactionObj.TilePos.y + 0.5f) * MapManager.Instance.TileSize, interactionObj.Z);
                npc.GetComponent<IT_NpcFunc>().Init((IT_NpcObj)interactionObj);
                AllInteraction.Add(npc);
                break;
        }
    }
}
