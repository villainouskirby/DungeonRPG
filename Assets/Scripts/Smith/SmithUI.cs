using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmithUI : SlotInteractHandler
{
    [Header("Smith")]
    [SerializeField] private Smith _smith;

    [Header("Smith PopUpUI")]
    [SerializeField] private SmithPopUpUI _smithPopUpUI;

    [Header("Prefab")]
    [SerializeField] private GameObject _smithSlotPrefab;

    [Header("Slot Content")]
    [SerializeField] private Transform _armorContent;
    [SerializeField] private Transform _weaponContent;
    [SerializeField] private Transform _toolContent;

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

    public void InitSmithUI()
    {

    }


    private int GetSmithSlotIndex(SmithSlotUI slot) => _exposedSlots.IndexOf(slot);

    #region Pointer Event

    public override void OnLeftClick()
    {
        _smithPopUpUI.SetInfo(_smith.GetSmithData(GetSmithSlotIndex(_pointedSlot as SmithSlotUI)));
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
