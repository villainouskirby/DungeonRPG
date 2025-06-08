public enum TileMapOptionEnum
{
    WallColliderManager = 0,
    InteractionObjManager = 1,
    SpawnerManager = 2,
    MapVisitedChecker = 3,
    CameraAreaManager = 4,
    End,
}

public enum TileMapOptionPrimeEnum
{
    WallColliderManager = 3,
    InteractionObjManager = 4,
    SpawnerManager = 2,
    MapVisitedChecker = 1,
    CameraAreaManager = 5,
}

public enum TileMapBasePrimeEnum
{
    DataLoader =            1,
    PlayerMoveChecker =     DataLoader + 1,
    ChunkManager =          PlayerMoveChecker + 1,
    MapManager =            ChunkManager + 1,
    DecoManager =           MapManager + 1,
    ShadowManager = DecoManager + 1,
}