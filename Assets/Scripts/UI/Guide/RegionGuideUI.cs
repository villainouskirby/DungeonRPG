using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegionGuideUI : SlotInteractHandler
{
    [SerializeField] private MapGuideUI _mapGuideUI;

    [SerializeField] private TextMeshProUGUI _regionNameText;

    // 슬롯 프리팹 및 부모 Transform
    [SerializeField] private GameObject _regionSlotPrefab;
    [SerializeField] private Transform _monsterContent;
    [SerializeField] private Transform _gatheringItemContent;
    [SerializeField] private Transform _confirmedGatheringItemContent;

    private List<RegionGuideSlotUI> _monsterSlot = new List<RegionGuideSlotUI>();
    private List<RegionGuideSlotUI> _gatheringItemSlot = new List<RegionGuideSlotUI>();
    private List<RegionGuideSlotUI> _comfirmedGatheringItemSlot = new List<RegionGuideSlotUI>();

    private int _currentRegionID;

    public void SetInfo(int mapIndex, int regionIndex)
    {
        if (_currentRegionID == regionIndex)
            return;

        _currentRegionID = regionIndex;

        RegionGuide regionGuide = _mapGuideUI.GetRegionGuideInfo(mapIndex, regionIndex);

        InitInfo(regionGuide);
    }

    private void InitInfo(RegionGuide regionGuide)
    {
        RemoveSlots(_monsterSlot);
        RemoveSlots(_gatheringItemSlot);
        RemoveSlots(_comfirmedGatheringItemSlot);
    }

    private void RemoveSlots(List<RegionGuideSlotUI> slots)
    {
        for (int i = slots.Count; i >= 0; i--)
        {
            Destroy(slots[i]);
            slots.RemoveAt(i);
        }
    }

    #region Pointer Event

    public override void OnLeftClick()
    {

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
