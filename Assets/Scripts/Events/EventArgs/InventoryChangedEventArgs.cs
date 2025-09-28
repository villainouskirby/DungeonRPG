namespace Events
{
    public class InventoryChangedEventArgs : EventArgs<InventoryChangedEventArgs>
    {
        public ItemData ItemData { get; private set; }
        public int Amount { get; private set; }

        public void Init(ItemData itemData, int amount)
        {
            ItemData = itemData;
            Amount = amount;
        }

        public override void Clear()
        {
            ItemData = null;
            Amount = 0;
        }
    }
}
