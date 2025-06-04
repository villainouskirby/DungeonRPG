using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ResourceNode_Info;

public class ResourceNodeTester : MonoBehaviour
{
    public GameObject ResourceNodePrefab;
    public Transform Root;

    [ContextMenu("Generate All ReosurceNode")]
    public void InitAllResourceNode()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("FarmHpBar");
        ResourceNodeBase.HpBarPrefab = handle.WaitForCompletion();

        for (int i = 0; i < ResourceNode.Length; i++)
        {
            GameObject resourceNode = Instantiate(ResourceNodePrefab, Root);
            resourceNode.transform.position = new(i * 2 - 1 + 5, 0, 0);
            ResourceNodeBase resourceNodeBase = resourceNode.GetComponent<ResourceNodeBase>();

            resourceNodeBase.Set(ResourceNode[i]);
        }
    }
}
