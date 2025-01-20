public abstract class Item
{
    public ItemData Data { get; private set; }
    public Item(ItemData data) => Data = data;

    public abstract Item Clone(int amount = 1);
}
