using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlantSpawnerTile : SpawnerTile
{
    [Header("Plant Settings")]
    public PlantEnum[]     Plants;

    public void Awake()
    {
    }
}
