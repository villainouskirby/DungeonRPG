using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTileManager : MonoBehaviour
{
    public static SpawnerTileManager Instance { get { return _instance; } }
    private static SpawnerTileManager _instance;


    private void Awake()
    {
        _instance = this;
    }


}
