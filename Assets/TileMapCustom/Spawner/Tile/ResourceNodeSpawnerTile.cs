using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceNodeSpawnerTile : SpawnerTile
{
    [Header("ResourceNode Settings")]
    [HideInInspector]
    public string[] ResourceNodes = new string[0];
}
