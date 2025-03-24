using UnityEngine;




public  class DataBaseNameViewer : MonoBehaviour
{
    public DataBaseName_TestClass[] TestClassDataViewer;
    public DataBaseName_Sheet1[] Sheet1DataViewer;
    void Start()
    {
        TestClassDataViewer = DataBaseName.TestClass;
        Sheet1DataViewer = DataBaseName.Sheet1;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"DataBaseNameViewer");
        viewer.AddComponent<DataBaseNameViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
