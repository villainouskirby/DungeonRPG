using UnityEngine;




public  class ResourceNode_InfoViewer : MonoBehaviour
{
    public ResourceNode_Info_ResourceNode[] ResourceNodeDataViewer;
    public ResourceNode_Info_ResourceNode_DropTable[] ResourceNode_DropTableDataViewer;
    void Start()
    {
        ResourceNodeDataViewer = ResourceNode_Info.ResourceNode;
        ResourceNode_DropTableDataViewer = ResourceNode_Info.ResourceNode_DropTable;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"ResourceNode_InfoViewer");
        viewer.AddComponent<ResourceNode_InfoViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
