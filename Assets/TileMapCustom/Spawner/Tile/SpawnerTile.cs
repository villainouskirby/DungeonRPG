using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTile : MonoBehaviour
{
    [Header("Spawner Group Settings")]
    public SpawnerGroupEnum     Group = SpawnerGroupEnum.A;
    public SpawnerCaseEnum      Case = SpawnerCaseEnum.All;
    [HideInInspector]
    public SpawnerType          Type;

    [Header("Custom Spawn Condition Settings")]
    public bool                 CustomSpawn;
    public float                MinRange;
    public float                MaxRange;

    [Header("Respawn Settings")]
    public float                CoolTime = 3f;


    void OnDrawGizmos()
    {
        Gizmos.color = Group switch
        {
            SpawnerGroupEnum.A => Color.red,
            SpawnerGroupEnum.B => Color.blue,
            SpawnerGroupEnum.C => Color.green,
            _ => Color.white,
        };
        Vector3 offset = new(0.5f, 0.3f, 0);
        Gizmos.DrawCube(transform.position + offset, Vector3.one * 0.3f);

        Gizmos.color = Case switch
        {
            SpawnerCaseEnum.All => Color.black,
            SpawnerCaseEnum.One => Color.red,
            SpawnerCaseEnum.Two => Color.blue,
            SpawnerCaseEnum.Three => Color.green,
            SpawnerCaseEnum.Four => Color.yellow,
            _ => Color.white,
        };
        offset = new(0.5f, -0.2f, 0);
        Gizmos.DrawCube(transform.position + offset, Vector3.one * 0.3f);
    }
}
