using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class InteractionObjManager : MonoBehaviour
{
    public static string DataFilePath = "Interaction/Prefabs/";

    public MapEnum MapType;

    private GameObject _root;

    public List<GameObject> AllInteraction;
    private InteractionObjData _interactionObjData;

    private float _tileSize;
    private GameObject _player;
    private Vector2Int _lastTile;


    void Awake()
    {
        _root = GameObject.Find("@InteractionObj");
        SetDataAsset(MapType.ToString());
    }

    private void Start()
    {
        _tileSize = MapManager.Instance.TileSize;
        _player = MapManager.Instance.Player;
        _lastTile = new(0, 0);
        Init();
    }

    void FixedUpdate()
    {
        _tileSize = MapManager.Instance.TileSize;
        Vector2Int newTile = GetCurrentTilePos();

        if (newTile != _lastTile)
        {
            _lastTile = newTile;
        }
    }

    Vector2Int GetCurrentTilePos()
    {
        int tileX = Mathf.FloorToInt(_player.transform.position.x / _tileSize);
        int tileY = Mathf.FloorToInt(_player.transform.position.y / _tileSize);
        return new Vector2Int(tileX, tileY);
    }

    public void Init()
    {
        for(int i = 0; i < _interactionObjData.Interaction.Count; i++)
        {
            GenerateInteractionObj(_interactionObjData.Interaction[i]);
        }
    }

    private void GenerateInteractionObj(InteractionObj interactionObj)
    {
        switch (interactionObj.Type)
        {
            case InteractionEnum.Entry:
                GameObject interaction = Instantiate(
                    Resources.Load<GameObject>($"{DataFilePath}IT_{interactionObj.Type.ToString()}")
                );

                interaction.transform.parent = _root.transform;
                interaction.transform.position = new((interactionObj.TilePos.x + 0.5f) * _tileSize, (interactionObj.TilePos.y + 0.5f) * _tileSize);
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
