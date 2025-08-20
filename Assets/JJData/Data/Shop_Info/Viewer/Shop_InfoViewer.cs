using UnityEngine;




public  class Shop_InfoViewer : MonoBehaviour
{
    public Shop_Info_Shop[] ShopDataViewer;
    void Start()
    {
        ShopDataViewer = Shop_Info.Shop;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"Shop_InfoViewer");
        viewer.AddComponent<Shop_InfoViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
