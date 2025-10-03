using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using Tutorial;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.GraphicsBuffer;

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
            InEvent(collider);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            OutEvent(collider);
        }
    }

    public void Set(EventAreaData data)
    {
        Data = data;
        _collider.size = Data.Size;
        _collider.offset = Data.Offset;
        transform.position = new(Data.Pos.x, Data.Pos.y, 0);
    }

    private void InEvent(Collider2D collider)
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
            case EventAreaType.OnOffSmoothLayer:
                OnOffSmoothLayer_In(collider);
                break;
            case EventAreaType.ChangeGroundLayer:
                ChangeGroundLayer_In();
                break;
            case EventAreaType.ChangePlayerLight:
                ChangePlayerLight_In();
                break;
            case EventAreaType.AutoSave:
                AutoSave_In();
                break;
        }
    }

    private void OutEvent(Collider2D collider)
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
            case EventAreaType.OnOffSmoothLayer:
                OnOffSmoothLayer_Out(collider);
                break;
            case EventAreaType.ChangeGroundLayer:
                break;
            case EventAreaType.ChangePlayerLight:
                break;
            case EventAreaType.AutoSave:
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
        UIPopUpHandler.Instance.GetScript<KeyGuideUI>().OpenTutorial(text);
    }

    private void ShowText_Out()
    {
        UIPopUpHandler.Instance.GetScript<KeyGuideUI>().CloseTutorial();
    }

    [Range(0, 1f)] public float process;   // 0~1 진행도(실시간)
    Transform _target;
    Coroutine _loop;
    bool _vertical;
    bool _forward;

    public void OnOffSmoothLayer_In(Collider2D other)
    {
        string[] targetLayers = Data.param1.Split('/');
        List<SpriteRenderer> layers = new();
        _target = TileMapMaster.Instance.Player.transform;

        foreach (string targetLayer in targetLayers)
        {
            if (targetLayer != "")
            {
                SpriteRenderer layer = MapManager.Instance.GetLayerObj(int.Parse(targetLayer.Trim())).GetComponent<SpriteRenderer>();
                layers.Add(layer);
                layer.gameObject.SetActive(true);
            }
        }

        string d = Data.param2;
        if (d.Equals("Up", StringComparison.OrdinalIgnoreCase)) { _vertical = true; _forward = true; }
        else if (d.Equals("Down", StringComparison.OrdinalIgnoreCase)) { _vertical = true; _forward = false; }
        else if (d.Equals("Right", StringComparison.OrdinalIgnoreCase)) { _vertical = false; _forward = true; }
        else if (d.Equals("Left", StringComparison.OrdinalIgnoreCase)) { _vertical = false; _forward = false; }

        // 이전 루프 정리
        if (_loop != null) { StopCoroutine(_loop); _loop = null; }

        // --- 로컬 코루틴: 트리거 안에 있는 동안 매 프레임 진행도 계산 ---
        IEnumerator Loop()
        {
            var myCol = GetComponent<BoxCollider2D>();
            if (myCol == null) yield break;

            while (_target != null)
            {
                // 내 트리거의 AABB (축 정렬 바운즈) — 회전 트리거는 아래 주석 참고
                Bounds b = myCol.bounds;

                // 대상 월드 위치를 한 축으로 투영 (Up/Down→y축, Left/Right→x축)
                Vector3 p = _target.position;
                float v = _vertical ? p.y : p.x;

                // 축 범위(min~max)에서 위치를 0~1로 매핑
                float a = _vertical ? b.min.y : b.min.x;
                float z = _vertical ? b.max.y : b.max.x;

                // 규칙:
                //  - Up/Right  : 아래/왼(0) → 위/오른(1)  => InverseLerp(a, z, v)
                //  - Down/Left : 위/오른(0) → 아래/왼(1)  => InverseLerp(z, a, v)
                float t = _forward ? Mathf.InverseLerp(a, z, v)
                                   : Mathf.InverseLerp(z, a, v);

                process = Mathf.Clamp01(t);
                for (int i = 0; i < layers.Count; i++)
                {
                    Color oriColor = layers[i].color;
                    oriColor.a = process;
                    layers[i].color = oriColor;
                }

                yield return null; // 프레임마다 갱신
            }
        }

        _loop = StartCoroutine(Loop());
    }

    /// <summary>
    /// 2D 트리거에서 "나갔을 때" 호출 (OnTriggerExit2D에서 호출)
    /// </summary>
    public void OnOffSmoothLayer_Out(Collider2D other)
    {
        if (_target == other.transform)
        {
            if (_loop != null) { StopCoroutine(_loop); _loop = null; }
            _target = null;
            bool isOne = process >= 0.5f;
            process = isOne ? 1f : 0f;

            string[] targetLayers = Data.param1.Split('/');
            List<SpriteRenderer> layers = new();

            foreach (string targetLayer in targetLayers)
            {
                if (targetLayer != "")
                {
                    SpriteRenderer layer = MapManager.Instance.GetLayerObj(int.Parse(targetLayer.Trim())).GetComponent<SpriteRenderer>();
                    layers.Add(layer);
                    layer.gameObject.SetActive(true);
                }
            }

            if (isOne)
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    layers[i].gameObject.SetActive(true);
                    Color oriColor = layers[i].color;
                    oriColor.a = 1;
                    layers[i].color = oriColor;
                }
            }
            else
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    layers[i].gameObject.SetActive(false);
                    Color oriColor = layers[i].color;
                    oriColor.a = 0;
                    layers[i].color = oriColor;
                }
            }
        }
    }

    private void ChangeGroundLayer_In()
    {
        int targetLayer = int.Parse(Data.param1);
        if (targetLayer == HeightManager.Instance.GroundLayer)
            return;
        HeightManager.Instance.ChangeLayer(targetLayer);
    }
    private void ChangePlayerLight_In()
    {
        Light2D light = TileMapMaster.Instance.Player.transform.GetChild(2).GetComponent<Light2D>();
        if (Data.param1.Trim() != "")
            light.pointLightOuterRadius = int.Parse(Data.param1);
        if (Data.param2.Trim() != "")
            light.intensity = int.Parse(Data.param2);
    }

    Coroutine _autoSaveC;
    private void AutoSave_In()
    {
        SaveManager.AutoSave();
        TMP_Text target = GameObject.Find("AutoSaveText").GetComponent<TMP_Text>();
        target.text = "자동 저장 중..";

        int fadeDuration = 1;
        int stayDuration = 1;

        if (_loop != null) { return; }

        IEnumerator Loop()
        {
            Color c = target.color;

            while (true)
            {
                // Fade In
                float t = 0f;
                while (t < fadeDuration)
                {
                    t += Time.deltaTime;
                    c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
                    target.color = c;
                    yield return null;
                }

                yield return new WaitForSeconds(stayDuration);
                target.text = "자동 저장 완료";
                // Fade Out
                t = 0f;
                while (t < fadeDuration)
                {
                    t += Time.deltaTime;
                    c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
                    target.color = c;
                    yield return null;
                }

                yield return new WaitForSeconds(stayDuration);
            }
        }

        _loop = StartCoroutine(Loop());
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
    ChangeLayer         = 0,
    ShowText            = 1,
    OnOffLayer          = 2,
    OnOffSmoothLayer    = 3,
    ChangeGroundLayer   = 4,
    ChangePlayerLight   = 5,
    AutoSave            = 6,
}