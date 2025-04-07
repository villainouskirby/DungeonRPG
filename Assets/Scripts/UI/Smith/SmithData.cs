[System.Serializable]
public class SmithData
{
    public long ID;
    public string Position;
    public long ResultItemID;
    public long[] IngredientItemIDs = new long[4];
    public int[] IngredientAmounts = new int[4];
    public bool IsActive;
}
