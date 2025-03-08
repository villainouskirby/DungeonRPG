using System.Collections.Generic;
using UnityEngine;

public class MapGuideUI : SlotInteractHandler
{
    [SerializeField] private MapGuide _mapGuide;
    [SerializeField] private RegionGuideUI _regionGuideUI;

    [SerializeField] private int _currentMapIndex;

    [SerializeField] private List<MapGuideSlotUI> _firstMapSlot;
    [SerializeField] private List<MapGuideSlotUI> _secondMapSlot;
    [SerializeField] private List<MapGuideSlotUI> _thirdMapSlot;

    private int GetSlotIndex(MapGuideSlotUI slot)
    {
        return (_currentMapIndex switch
        {
            0 => _firstMapSlot,
            1 => _secondMapSlot,
            2 => _thirdMapSlot,
            _ => _firstMapSlot
        }).IndexOf(slot);
    }

    public RegionGuide GetRegionGuideInfo(int mapIndex, int regionIndex)
    {
        return _mapGuide.GetRegionGuideInfo(mapIndex, regionIndex);
    }

    #region Pointer Event

    public override void OnLeftClick()
    {
        _regionGuideUI.SetInfo(_currentMapIndex, GetSlotIndex(_pointedSlot as MapGuideSlotUI));
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
