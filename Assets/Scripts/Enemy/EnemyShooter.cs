using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float projectileSpeed = 5f;
    private float shootTimer = 0f;

    void Update()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            ShootProjectile();
            shootTimer = 0f;
        }
    }

    void ShootProjectile()
    {
        // 투사체 prefab 생성
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        EnemyProjectile enemyProj = proj.GetComponent<EnemyProjectile>();

        if (enemyProj != null)
        {
            // 예시: 플레이어를 향해 발사 (플레이어는 태그 "Player"로 지정)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 dir = (player.transform.position - transform.position).normalized;
                enemyProj.Initialize(dir);
                enemyProj.speed = projectileSpeed;
            }
        }
    }
}
