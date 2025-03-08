using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "MapGuideInfo", menuName = "GuideInfo/MapGuideInfo")]
public class RegionGuideInfoSO : ScriptableObject
{
    public Sprite Sprite;
    public int Rank;
    [Multiline]
    public string Description;
    public Feature[] Features = new Feature[3];
    public Item[] Loots = new Item[3];
}
