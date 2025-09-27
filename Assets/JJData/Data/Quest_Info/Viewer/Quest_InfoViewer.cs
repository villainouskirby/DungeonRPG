using UnityEngine;




public  class Quest_InfoViewer : MonoBehaviour
{
    public Quest_Info_Quest[] QuestDataViewer;
    public Quest_Info_Gathering[] GatheringDataViewer;
    public Quest_Info_Hunting[] HuntingDataViewer;
    public Quest_Info_Investigation[] InvestigationDataViewer;
    void Start()
    {
        QuestDataViewer = Quest_Info.Quest;
        GatheringDataViewer = Quest_Info.Gathering;
        HuntingDataViewer = Quest_Info.Hunting;
        InvestigationDataViewer = Quest_Info.Investigation;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"Quest_InfoViewer");
        viewer.AddComponent<Quest_InfoViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
