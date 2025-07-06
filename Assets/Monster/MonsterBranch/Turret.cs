using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Behaviours/Attack/Turret Burst")]
public class TurretBurstBehaviour : AttackBehaviourSO
{
    public int bulletCount = 3;
    public float interval = .2f;

    public override IEnumerator Execute(MonsterContext ctx)
    {
        for (int i = 0; i < bulletCount; ++i)
        {
            SpawnBullet(ctx.transform.position, (ctx.player.position - ctx.transform.position).normalized);
            yield return new WaitForSeconds(interval);
        }
    }
    void SpawnBullet(Vector3 pos, Vector2 dir) { /* … */ }
}