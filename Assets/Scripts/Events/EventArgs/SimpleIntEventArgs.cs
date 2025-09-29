namespace Events
{
    public class SimpleIntEventArgs : EventArgs<SimpleIntEventArgs>
    {
        public int Value { get; private set; }

        public void Init(int value)
        {
            Value = value;
        }
        public override void Clear()
        {
            Value = 0;
        }
    }
}