using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mission : MonoBehaviour
{
    public QuestType Type;
    public int ID;
    public string Name;
    public Sprite Sprite;

    public int MaxProgress;
    public int Progress;

    public bool IsMissionCleared => Progress >= MaxProgress;
}
