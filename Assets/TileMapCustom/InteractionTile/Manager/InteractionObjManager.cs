using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TM = TileMapMaster;

public class InteractionObjManager : MonoBehaviour, ITileMapOption
{
    public static string DataFilePath = "Interaction/Prefabs/";

    private GameObject _root;

    public List<GameObject> AllInteraction;
    private InteractionObjData _interactionObjData;

    private bool _isActive = false;

    public int Prime { get { return (int)TileMapOptionPrimeEnum.InteractionObjManager; } }

    public void Init()
    {
        _root = GameObject.Find("@InteractionObj");
    }

    public void InitMap(MapEnum mapType)
    {
        ResetInteractionObj();

        SetDataAsset(mapType.ToString());

        for (int i = 0; i < _interactionObjData.Interaction.Count; i++)
        {
            GenerateInteractionObj(_interactionObjData.Interaction[i]);
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
                interaction.transform.position = new((interactionObj.TilePos.x + 0.5f) * MapManager.Instance.TileSize, (interactionObj.TilePos.y + 0.5f) * MapManager.Instance.TileSize);
                interaction.GetComponent<IT_EntryFunc>().Init((IT_EntryObj)interactionObj);
                break;
        }
    }

    private void SetDataAsset(string assetName)
    {
        string dataFilePath = $"{InteractionObjExtractor.DataFileDirectory}{assetName}/";
        _interactionObjData = Instantiate(Resources.Load<InteractionObjData>($"{dataFilePath}{assetName}InteractionObjData"));
    }
}
