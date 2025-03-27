[System.Serializable]
public abstract class Item
{
    public ItemData Data { get; private set; }
    public Item(ItemData data) => Data = data;

    public abstract Item Clone(); // TODO => 템들 이동방식 구현하면서 수정해야 할듯
}
