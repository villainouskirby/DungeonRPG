using UnityEngine;

[System.Serializable]
public class Mission
{
    // TODO => progress를 어떻게 올리게 할지 생각해야함 => 아마 ID별로 구분시켜 할듯함
    public QuestType Type;
    public int ID;
    public string Content;
    public Sprite Sprite;

    public int MaxProgress;
    public int Progress;

    public bool IsMissionCleared => Progress >= MaxProgress;
}
