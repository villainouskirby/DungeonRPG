using System.Collections.Generic;
using UnityEngine;

public class MapGuide : MonoBehaviour
{
    [SerializeField] private MapGuideUI _mapGuideUI;

    private List<RegionGuide> _firstMapInfo = new List<RegionGuide>();
    private List<RegionGuide> _secondMapInfo = new List<RegionGuide>();
    private List<RegionGuide> _thirdMapInfo = new List<RegionGuide>();

    public RegionGuide GetRegionGuideInfo(int mapIndex, int regionIndex)
    {
        List<RegionGuide> regionInfo = mapIndex switch
        {
            0 => _firstMapInfo,
            1 => _secondMapInfo,
            2 => _thirdMapInfo,
            _ => _firstMapInfo
        };

        return (regionIndex >= 0 && regionIndex < regionInfo.Count) ? regionInfo[regionIndex] : null;
    }
}
