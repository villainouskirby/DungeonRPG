using UnityEngine;




public  class SmithyTreeViewer : MonoBehaviour
{
    public SmithyTree_SmithyTree_Tool[] SmithyTree_ToolDataViewer;
    public SmithyTree_SmithyTree_Tool_Setting[] SmithyTree_Tool_SettingDataViewer;
    public SmithyTree_SmithyTree_Weapon[] SmithyTree_WeaponDataViewer;
    public SmithyTree_SmithyTree_Weapon_Setting[] SmithyTree_Weapon_SettingDataViewer;
    public SmithyTree_SmithyTree_Armor[] SmithyTree_ArmorDataViewer;
    public SmithyTree_SmithyTree_Armor_Setting[] SmithyTree_Armor_SettingDataViewer;
    void Start()
    {
        SmithyTree_ToolDataViewer = SmithyTree.SmithyTree_Tool;
        SmithyTree_Tool_SettingDataViewer = SmithyTree.SmithyTree_Tool_Setting;
        SmithyTree_WeaponDataViewer = SmithyTree.SmithyTree_Weapon;
        SmithyTree_Weapon_SettingDataViewer = SmithyTree.SmithyTree_Weapon_Setting;
        SmithyTree_ArmorDataViewer = SmithyTree.SmithyTree_Armor;
        SmithyTree_Armor_SettingDataViewer = SmithyTree.SmithyTree_Armor_Setting;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"SmithyTreeViewer");
        viewer.AddComponent<SmithyTreeViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
