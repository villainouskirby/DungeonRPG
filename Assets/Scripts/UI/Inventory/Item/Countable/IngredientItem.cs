public class IngredientItem : CountableItem
{
    public IngredientItemData IngredientItemData { get; private set; }

    public IngredientItem(IngredientItemData data, int amount = 1) : base(data, amount)
    {
        IngredientItemData = data;
    }

    public override Item Clone(int amount)
    {
        return new IngredientItem(IngredientItemData, amount);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
