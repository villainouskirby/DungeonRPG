namespace Events
{
    public class QuestCompleteEventArgs : EventArgs<QuestCompleteEventArgs>
    {
        public string QuestID { get; private set; }
        public string NPC { get; private set; }

        public void Init(string questID, string npc)
        {
            QuestID = questID;
            NPC = npc;
        }

        public override void Clear()
        {
            QuestID = "";
            NPC = "";
        }
    }
}
