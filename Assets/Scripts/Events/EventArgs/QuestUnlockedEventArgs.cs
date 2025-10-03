
namespace Events
{
    public class QuestUnlockedEventArgs : EventArgs<QuestUnlockedEventArgs>
    {
        public string QuestID { get; private set; }
        public string NPCName { get; private set; }

        public void Init(string id, string npcName)
        {
            QuestID = id;
            NPCName = npcName;
        }

        public override void Clear()
        {
            QuestID = "";
            NPCName = "";
        }
    }
}
