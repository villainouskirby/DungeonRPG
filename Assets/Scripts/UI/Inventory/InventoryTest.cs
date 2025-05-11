using UnityEngine;
using Cysharp.Threading.Tasks;

public class InventoryTest : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData testItemData;
    [SerializeField] private int amount = 1;

    [SerializeField] private ItemData[] allEquipment;

    [SerializeField] private ShopItemsSO shopItems;

    [SerializeField] private SmithDataSO smithData;

    [SerializeField] private QuickSlot quickSlot;


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
        AddInventory(testItemData);
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
        shopItems.Items.Add(new ShopItem(testItemData, 10));
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
