using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ResourceNode_Info;

public static class ResourceNodeListManager
{
    public static string[] ResourceNodeList;

    static ResourceNodeListManager()
    {
        ResourceNode_InfoDataParser.SetXlsxData();

        ResourceNodeList = new string[ResourceNode.Length];
        for (int i = 0; i < ResourceNode.Length; i++)
        {
            ResourceNodeList[i] = ResourceNode[i].ResourceNode_name;
        }
    }
}
