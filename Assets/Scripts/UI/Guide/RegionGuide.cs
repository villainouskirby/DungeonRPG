using System.Collections.Generic;
using UnityEngine;

public class RegionGuide : MonoBehaviour
{
    // 이거 하나로 줄이고 각 항목에 각각 적용하게 수정해야할듯
    public List<RegionGuideInfoSO> MonsterInfo = new List<RegionGuideInfoSO>();
    public List<RegionGuideInfoSO> GatheringItemInfo = new List<RegionGuideInfoSO>();
    public List<RegionGuideInfoSO> ConfirmedGatheringItemInfo = new List<RegionGuideInfoSO>();
}
