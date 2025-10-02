
namespace Events
{
    public class QuestUnlockedEventArgs : EventArgs<QuestUnlockedEventArgs>
    {
        public string NPCName { get; private set; }
        public string QuestID { get; private set; }

        public void Init(string npcName, string id)
        {
            NPCName = npcName;
            QuestID = id;
        }

        public override void Clear()
        {
            NPCName = "";
            QuestID = "";
        }
    }
}
