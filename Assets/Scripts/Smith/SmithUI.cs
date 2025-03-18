using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmithUI : SlotInteractHandler
{
    [Header("Smith")]
    [SerializeField] private Smith _smith;

    [Header("Smith PopUpUI")]
    [SerializeField] private SmithPopUpUI _smithPopUpUI;

    [Header("Prefab")]
    [SerializeField] private GameObject _smithSlotPrefab;

    [Header("Slot Content")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _armorContent;
    [SerializeField] private RectTransform _weaponContent;
    [SerializeField] private RectTransform _toolContent;

    [Header("Category Button")]
    [SerializeField] private Button _armorButton;
    [SerializeField] private Button _weaponButton;
    [SerializeField] private Button _toolButton;

    [Header("Excess PopUp")]
    [SerializeField] private ExcessPopUpUI _excessPopUp;

    /// <summary>
    /// 대장장이 카테고리
    /// <para/> bodyArmor legArmor를 합쳐서 bodyArmor로 표현해 방어구 카테고리를 나타냄
    /// </summary>
    private EquipmentType _category;
    public EquipmentType Category => _category;

    private List<SmithSlotUI> _armorSlots = new List<SmithSlotUI>();
    private List<SmithSlotUI> _weaponSlots = new List<SmithSlotUI>();
    private List<SmithSlotUI> _toolSlots = new List<SmithSlotUI>();
    private List<SmithSlotUI> _exposedSlots => _category switch
    {
        EquipmentType.bodyArmor => _armorSlots,
        EquipmentType.weapon => _weaponSlots,
        EquipmentType.tool => _toolSlots,
        _ => _armorSlots
    };

    private GameObject _currentContent;

    private void Start()
    {
        _currentContent = _armorContent.gameObject;

        _armorButton.onClick.AddListener(() => OpenContent(_armorContent));
        _weaponButton.onClick.AddListener(() => OpenContent(_weaponContent));
        _toolButton.onClick.AddListener(() => OpenContent(_toolContent));

        _armorContent.gameObject.SetActive(false);
        _weaponContent.gameObject.SetActive(false);
        _toolContent.gameObject.SetActive(false);
        OpenContent(_armorContent);

        OpenSmith();
    }

    /// <summary> 대장장이 UI 열기 (초기화라던가 켤때마다 작업할게 있어서 이거로 호출해야함) </summary>
    public void OpenSmith()
    {
        _smithPopUpUI.InitPopUpUI();
        _smithPopUpUI.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    /// <summary> 인벤/창고에 있는 재료 양 </summary>
    public int GetIngredientAmount(long ID)
    {
        return _smith.GetIngredientAmount(ID);
    }

    /// <summary> 제작 가능 여부 확인 => 인벤의 여유 공간 확인 </summary>
    public bool CheckCanCraft()
    {
        if (_smith.CheckCanCraft())
        {
            return true;
        }
        else
        {
            _excessPopUp.OpenExcessPopUp();
            return false;
        }
    }

    /// <summary> 제작된 아이템 인벤토리에 추가 </summary>
    public void AddCraftedItemToInventory(SmithData smithData)
    {
        _smith.AddCraftedItemToInventory(smithData);
    }

    /// <summary> 방금 인벤에 들어간 장비 사용(장착) </summary>
    public void EquipItem()
    {
        _smith.EquipItem();
    }

    private void InitUI(List<SmithData> datas, List<SmithSlotUI> targetList, Transform targetContent)
    {
        // 기존 데이터 제거
        foreach (var ui in targetList)
        {
            Destroy(ui.gameObject);
        }
        targetList.Clear();

        TreeLayoutManager layoutManager = targetContent.GetComponent<TreeLayoutManager>();

        // 슬롯 생성
        for (int i = 0; i < datas.Count; i++)
        {
            SmithSlotUI slotUI;
            GameObject newSlot = Instantiate(_smithSlotPrefab, targetContent);
            if ((slotUI = newSlot.GetComponent<SmithSlotUI>()) == null) slotUI = newSlot.AddComponent<SmithSlotUI>();
            targetList.Add(slotUI);

            slotUI.SetItemInfo(null, datas[i].IsActive); // TODO => DB연결해서 결과 아이템 ID에 맞는 이미지 가져오기
            layoutManager.SetPosition(slotUI.GetComponent<RectTransform>(), datas[i].Position);
        }
    }

    public void InitArmorUI(List<SmithData> armorDatas)
    {
        InitUI(armorDatas, _armorSlots, _armorContent);
    }

    public void InitWeaponUI(List<SmithData> weaponDatas)
    {
        InitUI(weaponDatas, _weaponSlots, _weaponContent);
    }

    public void InitToolUI(List<SmithData> toolDatas)
    {
        InitUI(toolDatas, _toolSlots, _toolContent);
    }

    public SmithData GetSmithData(int index)
    {
        return _smith.GetSmithData(index);
    }

    private void OpenContent(RectTransform content)
    {
        _currentContent.SetActive(false);
        _scrollRect.content = content;
        content.anchoredPosition = Vector2.zero;
        _currentContent = content.gameObject;
        _currentContent.SetActive(true);
    }

    private int GetSmithSlotIndex(SmithSlotUI slot) => _exposedSlots.IndexOf(slot);

    #region Pointer Event

    public override void OnLeftClick()
    {
        _smithPopUpUI.SetInfo(GetSmithSlotIndex(_pointedSlot as SmithSlotUI));
    }

    public override void OnDoubleClick()
    {
    }

    public override void OnPointerIn()
    {
    }

    public override void OnPointerOut()
    {
    }

    public override void OnRightClick()
    {
    }

    #endregion
}
