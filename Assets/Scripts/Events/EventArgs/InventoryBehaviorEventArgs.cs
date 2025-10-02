namespace Events
{
    public class InventoryBehaviorEventArgs : EventArgs<InventoryBehaviorEventArgs>
    {
        public enum Behavior
        {
            None,
            Drink,
            Register,
            Equip,
        }

        public Behavior InventoryBehavior { get; private set; }
        public string ItemID { get; private set; }

        public void Init(Behavior behavior, string id)
        {
            InventoryBehavior = behavior;
            ItemID = id;
        }

        public override void Clear()
        {
            InventoryBehavior = Behavior.None;
            ItemID = "";
        }
    }
}
