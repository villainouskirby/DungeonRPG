using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class Spawner
{
    public float        MinRange;
    public float        MaxRange;
    public Vector2Int   TilePos;
    public float        CoolTime;
    public SpawnerType  Type;

    public float        CurrentTime = 0;

    public bool         IsSpawn = false;
    public bool         IsIdentify = false;
    public GameObject   SpawnObj;

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
        SpawnObj = GetSpawnObject();
        SpawnGameObject(SpawnObj);
    }

    public virtual void CheckVisible()
    {
        if (!IsSpawn)
            return;
        if (IsIdentify)
            return;

        if (0 < MapManager.Instance.FOVCaster.IsInFOV(TilePos))
        {
            IsIdentify = true;
            SpawnObj.SetActive(true);
        }
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
        if (0 < MapManager.Instance.FOVCaster.IsInFOV(TilePos))
        {
            IsIdentify = true;
            target.SetActive(true);
        }
    }

    public virtual void Init()
    {
        SpawnerReset();
    }

    public virtual void SpawnerReset()
    {
        CurrentTime = 0;
        IsSpawn = false;
        IsIdentify = false;
        SpawnObj = null;
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
