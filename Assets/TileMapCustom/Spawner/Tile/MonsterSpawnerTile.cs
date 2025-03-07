using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnerTile : SpawnerTile
{
    [Header("Monster Settings")]
    public int              ActiveRange;
    public MonsterEnum[]    Monsters;
}
