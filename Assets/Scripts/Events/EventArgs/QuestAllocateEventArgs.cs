
namespace Events
{
    public class QuestAllocateEventArgs : EventArgs<QuestAllocateEventArgs>
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
