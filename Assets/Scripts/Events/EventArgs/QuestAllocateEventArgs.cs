
namespace Events
{
    public class QuestAllocateEventArgs : EventArgs<QuestAllocateEventArgs> // TODO => 퀘스트 args 하나로 통합해서 쓰는게 나을듯
    {
        public string QuestID { get; private set; }
        public string TargetNPC { get; private set; }

        public void Init(string id, string targetNPC)
        {
            QuestID = id;
            TargetNPC = targetNPC;
        }

        public override void Clear()
        {
            QuestID = "";
            TargetNPC = "";
        }
    }
}
