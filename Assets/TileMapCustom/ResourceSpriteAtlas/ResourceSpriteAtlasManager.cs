#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using static ResourceNode_Info;

[ExecuteInEditMode]
public class ResourceSpriteAtlasManager : MonoBehaviour
{
    public static ResourceSpriteAtlasManager Instance { get {  return _instance; } }
    private static ResourceSpriteAtlasManager _instance;

    [Header("Info  Settings")]
    public GameObject InfoPrefab;

    [Header("ResourceNode Settings")]
    public GameObject ResourceNodePrefab;
    public Transform Root;
    public SpriteAtlas SpriteAtlas;

    [Header("Sprite Settings")]
    public List<SpriteInfo> SpriteInfo = new();
    private Dictionary<string, SpriteInfo> _spriteInfoDic = new();

    private List<SrInfo> _sr;

    private bool _ready = false;
    
    private void Awake()
    {
        _instance = this;
        _sr = new();
        SpriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/TileMapAtals/Resource/ResourceNodeAtlas.spriteatlas");
        InitLastSprite();
    }

    private void InitLastSprite()
    {
        _sr = new();
        SpriteInfo = new();
        _spriteInfoDic = new();

        List<SpriteRenderer> srs = new();
        foreach (Transform child in Root)
        {
            srs.Add(child.GetComponent<SpriteRenderer>());
        }
        for (int i = 0; i < srs.Count; i++)
        {
            SrInfo srInfo = new(srs[i], srs[i].sprite, srs[i].name);
            _sr.Add(srInfo);
            SpriteInfo spriteInfo = new(srs[i].sprite, srs[i].name);
            SpriteInfo.Add(spriteInfo);
            _spriteInfoDic[srs[i].name] = spriteInfo;

            CheckChange checkChange = srs[i].gameObject.AddComponent<CheckChange>();
            checkChange.Init(srInfo, CheckSpriteChange);
        }
        
        _ready = true;
    }

    private void CheckSpriteChange(SrInfo srInfo)
    {
        if (srInfo.LastSprite != srInfo.Sr.sprite)
        {
            _spriteInfoDic[srInfo.Name].Sprite = srInfo.Sr.sprite;
            srInfo.LastSprite = srInfo.Sr.sprite;
        }
    }

    private void CheckInspectorSprite()
    {
        if (_sr == null)
            return;

        for (int i = 0; _sr.Count > i; i++)
        {
            if (_spriteInfoDic[_sr[i].Name].Sprite != _sr[i].Sr.sprite)
            {
                _sr[i].Sr.sprite = _spriteInfoDic[_sr[i].Name].Sprite;
                _sr[i].LastSprite = _spriteInfoDic[_sr[i].Name].Sprite;
            }
        }
    }


    [ContextMenu("Set All ResourceNode")]
    public void SetAllResourceNode()
    {
        _ready = false;
        SpriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/TileMapAtals/Resource/ResourceNodeAtlas.spriteatlas");
        SpriteAtlasUtility.PackAtlases(
            new[] { SpriteAtlas },
            EditorUserBuildSettings.activeBuildTarget);

        List<SpriteRenderer> childSr = new();
        foreach (Transform child in Root)
        {
            childSr.Add(child.GetComponent<SpriteRenderer>());
        }
        for (int i = 0; i < childSr.Count; i++)
            if (childSr[i] != null)
                DestroyImmediate(childSr[i].gameObject);

        ResourceNode_InfoDataParser.SetXlsxData();
        ResourceNodeBase.SpriteAtlas = SpriteAtlas;

        for (int i = 0; i < ResourceNode.Length; i++)
        {
            GameObject resourceNode = Instantiate(ResourceNodePrefab, Root);
            resourceNode.transform.position = new(i * 2 - 1 + 5, 0, 0);
            resourceNode.name = ResourceNode[i].ResourceNode_sprite;
            ResourceNodeBase resourceNodeBase = resourceNode.GetComponent<ResourceNodeBase>();

            resourceNodeBase.Init();
            resourceNodeBase.Set(ResourceNode[i]);

            GameObject info = Instantiate(InfoPrefab, resourceNode.transform);
            resourceNode.transform.position = new(i * 2 - 1 + 5, 1, 0);
            info.transform.GetChild(0).GetComponent<TMP_Text>().text = ResourceNode[i].ResourceNode_name;
            info.transform.GetChild(1).GetComponent<TMP_Text>().text = ResourceNode[i].ResourceNode_sprite;
        }

        InitLastSprite();
    }

    private void OnValidate()
    {
        if (!_ready)
            return;

        CheckInspectorSprite();
    }
}

[System.Serializable]
public class SpriteInfo
{
    public Sprite Sprite;
    public string Name;

    public SpriteInfo(Sprite sprite, string name)
    {
        Sprite = sprite;
        Name = name;
    }
}

public class SrInfo
{
    public SpriteRenderer Sr;
    public Sprite LastSprite;
    public string Name;

    public SrInfo(SpriteRenderer sr, Sprite lastSprite, string name)
    {
        Sr = sr;
        LastSprite = lastSprite;
        Name = name;
    }
}
#endif