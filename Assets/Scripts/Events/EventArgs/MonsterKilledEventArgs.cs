namespace Events
{
    public class MonsterKilledEventArgs : EventArgs<MonsterKilledEventArgs>
    {
        public string MonsterID { get; private set; }

        public void Init(string id)
        {
            MonsterID = id;
        }

        public override void Clear()
        {
            MonsterID = "";
        }
    }
}
