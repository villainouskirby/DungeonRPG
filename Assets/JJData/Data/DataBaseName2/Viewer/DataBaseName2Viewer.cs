using UnityEngine;




public  class DataBaseName2Viewer : MonoBehaviour
{
    public DataBaseName2_TestClass[] TestClassDataViewer;
    void Start()
    {
        TestClassDataViewer = DataBaseName2.TestClass;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"DataBaseName2Viewer");
        viewer.AddComponent<DataBaseName2Viewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
