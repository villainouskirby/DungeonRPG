using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnerData", menuName = "TileMap/SpawnerData")]
public class SpawnerData : ScriptableObject
{
    [SerializeReference]
    public Spawner[] Spawner;
}
