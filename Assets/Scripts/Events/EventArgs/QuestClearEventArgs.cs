namespace Events
{
    public class QuestClearEventArgs : EventArgs<QuestClearEventArgs>
    {
        public string QuestID { get; private set; }
        public bool IsClear { get; private set; }

        public void Init(string id, bool isClear)
        {
            QuestID = id;
            IsClear = isClear;
        }

        public override void Clear()
        {
            QuestID = "";
            IsClear = false;
        }
    }
}
