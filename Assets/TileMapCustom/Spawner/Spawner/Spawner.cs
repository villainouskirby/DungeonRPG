using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spawner
{
    public float        MinRange;
    public float        MaxRange;
    public Vector2Int   TilePos;
    public float        CoolTime;
    public SpawnerType  Type;

    public float        CurrentTime = 0;

    private bool        IsSpawn = false;

    public void TrySpawn()
    {
        if (IsSpawn) return;

        CurrentTime += Time.deltaTime;

        if (CurrentTime >= CoolTime)
        {
            IsSpawn = true;
            Spawn();
            CurrentTime = 0;
        }
    }

    public virtual void Spawn()
    {
        SpawnGameObject(GetSpawnObject());
    }

    public virtual GameObject GetSpawnObject()
    {
        return null;
    }

    public virtual void SpawnGameObject(GameObject target)
    {
        if (target == null)
            return;

        target.transform.position = new((TilePos.x + 0.5f) * MapManager.Instance.TileSize, (TilePos.y + 0.5f) * MapManager.Instance.TileSize, 0);
    }

    public virtual void Init()
    {
        CurrentTime = 0;
        IsSpawn = false;
    }

    public Spawner(SpawnerTile spawnerTile, float genericMinRange, float genericMaxRange)
    {
        int tileX = Mathf.FloorToInt((spawnerTile.transform.position.x + 9) / 1f);
        int tileY = Mathf.FloorToInt((spawnerTile.transform.position.y + 5) / 1f);
        TilePos = new(tileX, tileY);
        CoolTime = spawnerTile.CoolTime;
        Type = spawnerTile.Type;
        if (spawnerTile.CustomSpawn)
        {
            MinRange = spawnerTile.MaxRange;
            MaxRange = spawnerTile.MinRange;
        }
        else
        {
            MinRange = genericMinRange;
            MaxRange = genericMaxRange;
        }
    }
}
