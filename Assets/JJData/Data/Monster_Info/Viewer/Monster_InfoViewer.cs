using UnityEngine;




public  class Monster_InfoViewer : MonoBehaviour
{
    public Monster_Info_Monster[] MonsterDataViewer;
    public Monster_Info_Monster_Property_Table[] Monster_Property_TableDataViewer;
    public Monster_Info_Monster_Property[] Monster_PropertyDataViewer;
    public Monster_Info_Monster_Property_Effect[] Monster_Property_EffectDataViewer;
    public Monster_Info_Monster_DropTable[] Monster_DropTableDataViewer;
    public Monster_Info_Monster_Condition[] Monster_ConditionDataViewer;
    void Start()
    {
        MonsterDataViewer = Monster_Info.Monster;
        Monster_Property_TableDataViewer = Monster_Info.Monster_Property_Table;
        Monster_PropertyDataViewer = Monster_Info.Monster_Property;
        Monster_Property_EffectDataViewer = Monster_Info.Monster_Property_Effect;
        Monster_DropTableDataViewer = Monster_Info.Monster_DropTable;
        Monster_ConditionDataViewer = Monster_Info.Monster_Condition;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"Monster_InfoViewer");
        viewer.AddComponent<Monster_InfoViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
