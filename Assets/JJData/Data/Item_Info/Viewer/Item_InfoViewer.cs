using UnityEngine;




public  class Item_InfoViewer : MonoBehaviour
{
    public Item_Info_Item[] ItemDataViewer;
    public Item_Info_ThrowItem[] ThrowItemDataViewer;
    public Item_Info_DropItem[] DropItemDataViewer;
    public Item_Info_Weapon[] WeaponDataViewer;
    public Item_Info_Armor[] ArmorDataViewer;
    public Item_Info_Backpack[] BackpackDataViewer;
    public Item_Info_SubWeapon[] SubWeaponDataViewer;
    public Item_Info_Potion[] PotionDataViewer;
    public Item_Info_Hone[] HoneDataViewer;
    public Item_Info_Condition[] ConditionDataViewer;
    void Start()
    {
        ItemDataViewer = Item_Info.Item;
        ThrowItemDataViewer = Item_Info.ThrowItem;
        DropItemDataViewer = Item_Info.DropItem;
        WeaponDataViewer = Item_Info.Weapon;
        ArmorDataViewer = Item_Info.Armor;
        BackpackDataViewer = Item_Info.Backpack;
        SubWeaponDataViewer = Item_Info.SubWeapon;
        PotionDataViewer = Item_Info.Potion;
        HoneDataViewer = Item_Info.Hone;
        ConditionDataViewer = Item_Info.Condition;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateViewer()
    {
        GameObject viewer = new GameObject($"Item_InfoViewer");
        viewer.AddComponent<Item_InfoViewer>();
        GameObject.DontDestroyOnLoad(viewer);
    }
}
