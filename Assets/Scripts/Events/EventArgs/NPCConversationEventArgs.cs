namespace Events
{
    public class NPCConversationEventArgs : EventArgs<NPCConversationEventArgs>
    {
        public string NPCName { get; private set; }

        public void Init(string npcName)
        {
            NPCName = npcName;
        }
    
        public override void Clear()
        {
            NPCName = "";
        }
    }
}
