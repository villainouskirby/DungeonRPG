using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SaveData
{
    public MapEnum mapType; // 현재 맵
    public Vector3 PlayerPos; // 현재 플레이어 위치
    [SerializeReference]
    public List<ItemData> Items = new(); // 현재 플레이어 아이템
    public List<ResourceNodeSpawner> ResourceNodeSpawner = new();
}

public class ResourceNodeSaveData
{
    public Vector3 Pos;
    public float CurrentHp;
    public bool[] DropAble;
    public float DropHpCut;
}