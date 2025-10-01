using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tutorial;
using UnityEngine;

[System.Serializable]
public class EventArea : MonoBehaviour
{
    public EventAreaData Data;

    private BoxCollider2D _collider;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            InEvent();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            OutEvent();
        }
    }

    public void Set(EventAreaData data)
    {
        Data = data;
        _collider.size = Data.Size;
        _collider.offset = Data.Offset;
        transform.position = new(Data.Pos.x, Data.Pos.y, 0);
    }

    private void InEvent()
    {
        switch (Data.Type)
        {
            case EventAreaType.ChangeLayer:
                ChangeLayer_In();
                break;
            case EventAreaType.ShowText:
                ShowText_In();
                break;
            case EventAreaType.OnOffLayer:
                OnOffLayer_In();
                break;
        }
    }

    private void OutEvent()
    {
        switch (Data.Type)
        {
            case EventAreaType.ChangeLayer:
                break;
            case EventAreaType.ShowText:
                ShowText_Out();
                break;
            case EventAreaType.OnOffLayer:
                break;
        }
    }


    private void ChangeLayer_In()
    {
        int targetLayer = int.Parse(Data.param1);
        if (targetLayer == HeightManager.Instance.CurrentLayer)
            return;
        HeightManager.Instance.ChangeLayer(targetLayer);
    }

    private void OnOffLayer_In()
    {
        string[] onLayers = Data.param1.Split('/');
        string[] offLayers = Data.param2.Split('/');

        foreach (string onLayer in onLayers)
        {
            if (onLayer != "")
                MapManager.Instance.GetLayerObj(int.Parse(onLayer.Trim())).SetActive(true);
        }
        foreach (string offLayer in offLayers)
        {
            if (offLayer != "")
                MapManager.Instance.GetLayerObj(int.Parse(offLayer.Trim())).SetActive(false);
        }
    }

    private void ShowText_In()
    {
        string text = Data.param1;
        KeyGuideUI.GuideType type = (KeyGuideUI.GuideType)Enum.Parse(typeof(KeyGuideUI.GuideType), text);
        UIPopUpHandler.Instance.GetScript<KeyGuideUI>().OpenTutorial(type);
    }

    private void ShowText_Out()
    {
        UIPopUpHandler.Instance.GetScript<KeyGuideUI>().CloseTutorial();
    }
}

[System.Serializable]
public class EventAreaData
{
    [HideInInspector]
    public Vector2 Pos;
    [HideInInspector]
    public Vector2 Size;
    [HideInInspector]
    public Vector2 Offset;
    public EventAreaType Type;
    public string param1;
    public string param2;
    public string param3;
}

public enum EventAreaType
{
    ChangeLayer     = 0,
    ShowText        = 1,
    OnOffLayer      = 2,
}