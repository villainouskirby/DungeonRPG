public class TestItem : CountableItem
{
    public TestItemData TestData { get; private set; }
    public TestItem(TestItemData data, int amount = 1) : base(data, amount)
    {
        TestData = data;
    }

    public override Item Clone(int amount = 1)
    {
        return new TestItem(TestData, amount);
    }
}
