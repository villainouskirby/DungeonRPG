using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using static ResourceNode_Info;

public class ResourceNodeTester : MonoBehaviour
{
    public GameObject ResourceNodePrefab;
    public Transform Root;

    private void DeleteAll()
    {
        List<Transform> target = new();
        foreach(Transform child in Root)
            target.Add(child);

        for (int i = 0; i < target.Count; i++)
            DestroyImmediate(target[i].gameObject);
    }

    [ContextMenu("Generate All ReosurceNode for Editor")]
    public void GenerateInEditor()
    {
        DeleteAll();
        ResourceNode_InfoDataParser.SetXlsxData();

        ResourceNodeBase.SpriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/TileMapAtals/Resource/ResourceNodeAtlas.spriteatlas");
        SpriteAtlasUtility.PackAtlases(
            new[] { ResourceNodeBase.SpriteAtlas },
            EditorUserBuildSettings.activeBuildTarget);

        for (int i = 0; i < ResourceNode.Length; i++)
        {
            GameObject resourceNode = Instantiate(ResourceNodePrefab, Root);
            resourceNode.transform.position = new(i * 2 - 1 + 5, 0, 0);
            ResourceNodeBase resourceNodeBase = resourceNode.GetComponent<ResourceNodeBase>();

            resourceNodeBase.Init();
            resourceNodeBase.Set(ResourceNode[i]);
        }
    }

    [ContextMenu("Generate All ReosurceNode")]
    public void GenerateAllResourceNode()
    {
        DeleteAll();

        AsyncOperationHandle<SpriteAtlas> handle2 = Addressables.LoadAssetAsync<SpriteAtlas>("ResourceNodeAtlas");
        ResourceNodeBase.SpriteAtlas = handle2.WaitForCompletion();

        for (int i = 0; i < ResourceNode.Length; i++)
        {
            GameObject resourceNode = Instantiate(ResourceNodePrefab, Root);
            resourceNode.transform.position = new(i * 2 - 1 + 5, 0, 0);
            ResourceNodeBase resourceNodeBase = resourceNode.GetComponent<ResourceNodeBase>();

            resourceNodeBase.Init();
            resourceNodeBase.Set(ResourceNode[i]);
        }
    }
}
