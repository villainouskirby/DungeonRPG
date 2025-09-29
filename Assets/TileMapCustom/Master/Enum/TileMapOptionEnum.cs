public enum TileMapOptionEnum
{
    WallColliderManager = 0,
    InteractionObjManager = 1,
    SpawnerManager = 2,
    MapVisitedChecker = 3,
    CameraAreaManager = 4,
    EventAreaManager = 5,
    End,
}

public enum TileMapOptionPrimeEnum
{
    InteractionObjManager = 3,
    SpawnerManager = 2,
    MapVisitedChecker = 1,
    CameraAreaManager = 4,
    EventAreaManager = 5,
}

public enum TileMapBasePrimeEnum
{
    DataLoader =            0,
    PlayerMoveChecker =     DataLoader + 1,
    ChunkManager =          PlayerMoveChecker + 1,
    MapManager =            ChunkManager + 1,
    HeightManager =         MapManager + 1,
    DecoManager =           HeightManager + 1,
    WallManager =           DecoManager + 1,
    NavMeshManager =        WallManager + 1,
    ShadowManager =         NavMeshManager + 1,
}