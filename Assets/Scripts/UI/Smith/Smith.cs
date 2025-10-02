using System.Collections.Generic;
using UnityEngine;

public class Smith : NPCBase<Smith>
{
    [Header("Smith UI")]
    [SerializeField] private SmithUI _smithUI;

    [Header("Smith Data List")]
    [SerializeField] private SmithDataSO _armorList;
    [SerializeField] private SmithDataSO _weaponList;
    [SerializeField] private SmithDataSO _toolList;

    private List<SmithData> _armors => _armorList.SmithDatas;
    private List<SmithData> _weapons => _weaponList.SmithDatas;
    private List<SmithData> _tools => _toolList.SmithDatas;

    public SmithData GetSmithData(int index)
    {
        List<SmithData> list = _smithUI.Category switch
        {
            Equipment.EquipmentType.Armor => _armors,
            Equipment.EquipmentType.Weapon => _weapons,
            Equipment.EquipmentType.Backpack => _tools,
            _ => _armors
        };

        return (index < 0 || index >= list.Count) ? null : list[index];
    }

    [Header("Inventory Data")]
    [SerializeField] private Inventory _invenory;
    [SerializeField] private Storage _storage;

    protected override void Awake()
    {
        base.Awake();

        InitSmith();
    }

    /// <summary> 인벤/창고에 있는 재료 양 </summary>
    public int GetIngredientAmount(string SID)
    {
        return _invenory.GetItemAmountSum(SID) + _storage.GetItemAmountSum(SID);
    }

    /// <summary> 제작 가능 여부 확인 => 인벤의 여유 공간 확인 </summary>
    public bool CheckCanCraft()
    {
        return _invenory.RestCapacity > 0;
    }

    /// <summary> 제작된 아이템 인벤토리에 추가 </summary>
    public void AddCraftedItemToInventory(SmithData smithData)
    {
        // 재료 소모
        for (int i = 0; i < smithData.IngredientItemIDs.Length; i++)
        {
            /* ItemData itemData = smithData.IngredientItemIDs[i]; // DB에서 ID에 맞는 데이터 가져와야함
            int restAmount = _invenory.RemoveItem(itemData, smithData.IngredientAmounts[i]);
            if (restAmount > 0)
            {
                restAmount = _storage.RemoveItem(itemData, restAmount);

                if (restAmount > 0)
                {
                    Debug.Log("재료 부족 오류"); // 사실상 이거 발생하면 안되는건데 되면 유저과실이라 어떻게 할지는 정해야할듯
                    return;
                }
            }*/
        }

        // 아이템 인벤토리에 넣기
        //_invenory.AddItem();
    }

    /// <summary> 방금 인벤에 들어간 장비 사용(장착) </summary>
    public void EquipItem()
    {
        _invenory.UseItem();
    }

    protected override void InitBase()
    {
        UIPopUpHandler.Instance.RegisterUI(this);
    }

    public void InitSmith()
    {
        if (_armorList != null) _smithUI.InitArmorUI(_armors);
        if (_weaponList != null) _smithUI.InitWeaponUI(_weapons);
        if (_toolList != null) _smithUI.InitToolUI(_tools);
    }
}
