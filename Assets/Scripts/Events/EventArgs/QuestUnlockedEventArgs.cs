
namespace Events
{
    public class QuestUnlockedEventArgs : EventArgs<QuestUnlockedEventArgs>
    {
        public string NPCName { get; private set; }
        public string QuestID { get; private set; }

        public void Init(string name, string id)
        {
            NPCName = name;
            QuestID = id;
        }

        public override void Clear()
        {
            NPCName = "";
            QuestID = "";
        }
    }
}
