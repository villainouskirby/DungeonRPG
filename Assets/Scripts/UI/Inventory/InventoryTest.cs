using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using ItemUtility;

public class InventoryTest : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData testItemData;
    [SerializeField] private int amount = 1;

    [SerializeField] private ItemData[] allEquipment;

    [SerializeField] private Shop shop;

    [SerializeField] private SmithDataSO smithData;

    [SerializeField] private QuickSlot quickSlot;

    [SerializeField] private Sprite tempPotion1;
    [SerializeField] private Sprite tempPotion2;
    [SerializeField] private Sprite tempRock;

    [Header("Quest")]
    [SerializeField] private Quest quest;
    [SerializeField] private bool isMainQuest;
    [SerializeField] private Sprite sprite;


    public void AddQuickSlot()
    {
        quickSlot.AddToSlot(testItemData.Createitem());
    }

    public void AddInventory()
    {
        ItemData data = ItemDataConstructor.GetItemData("HER_CGR_LEA");
        AddInventory(data);
    }
    public Dictionary<string, Item_Info_Item> ItemDic;
    public void AddPotion1()
    {
        ItemDic = SheetDataUtil.DicByKey(Item_Info.Item, x => x.id);
        PotionItemData itemData = new(ItemDic["ITM_POT_001"], tempPotion1);
        UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(itemData);
    }
    public void AddPotion2()
    {
        ItemDic = SheetDataUtil.DicByKey(Item_Info.Item, x => x.id);
        PotionItemData itemData = new(ItemDic["ITM_POT_002"], tempPotion2);
        UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(itemData);
    }
    public void AddStoneItem()
    {
        ItemDic = SheetDataUtil.DicByKey(Item_Info.Item, x => x.id);
        ThrowItemData itemData = new(ItemDic["ITM_MIN_ROC"], tempRock, "PAR_MIN_ROC");
        UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(itemData);
    }

    public void AddDropItem()
    {
        var a = DropTableUtil.GetDropItemFromTable("ITM_MIN_CPR/100/2/3");
        UIPopUpHandler.Instance.GetUI<Inventory>().AddItem(a.data, a.amount);
    }
    public void AddInventory(ItemData data)
    {
        int rest = inventory.AddItem(data, amount);
        if (rest > 0) Debug.Log("들어가지 못한 아이템 개수 : " + rest);
    }
    
    
    public void AddAllEquipment()
    {
        foreach (ItemData data in allEquipment)
        {
            AddInventory(data);
        }
    }

    public void AddShopItem()
    {
        shop.AddShopItem(new TestItemData(Item_Info.Item[0], null).Createitem());
    }

    public void InitShop()
    {
        shop.InitShop();
    }

    public void ResetShop()
    {
        shop.ResetShop();
    }

    public void AddQuest()
    {
        Mission mission = new Mission();
        mission.Type = QuestType.gathering;
        mission.Content = "gathering";
        mission.Sprite = sprite;
        mission.MaxProgress = 20;
        mission.Progress = 10;

        QuestInfo info = new QuestInfo();
        info.IsMainQuest = isMainQuest;
        info.Missions = new Mission[3] { mission, mission, mission };
        info.QuestDescription = "quest description";
        info.Rewards = new Item[3] { testItemData.Createitem(), testItemData.Createitem(), testItemData.Createitem() };
        quest.AddQuest(info);
    }

    public void AddSmithData()
    {
        for (int i = 0; i < 4; i++)
        {
            smithData.SmithDatas.Add(new SmithData()
            {
                ID = i,
                Position = 'A' + (i+1).ToString(),
                ResultItemID = i,
                IsActive = false
            });

        }
        for (int i = 0; i < 4; i++)
        {
            smithData.SmithDatas.Add(new SmithData()
            {
                ID = i+4,
                Position = 'B' + (i + 1).ToString(),
                ResultItemID = i+4,
                IsActive = true
            });

        }
    }

    public void UniTaskTest()
    {
        DelayDebug().Forget();
    }

    public async UniTaskVoid DelayDebug()
    {
        Debug.Log("호출시작");
        await UniTask.Delay(2000);
        Debug.Log("호출된지 2초 후");
    }
}
