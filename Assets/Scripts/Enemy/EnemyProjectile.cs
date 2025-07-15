using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 20;
    public float speed = 5f;
    private Vector2 direction;

    // 투사체 발사 시 초기화 함수
    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }
    void Start()
    {
        // 탑뷰 환경이므로 중력이 적용되지 않도록 Rigidbody2D의 gravityScale을 0으로 설정
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
        // 5초 후 투사체를 자동으로 삭제
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 매 프레임마다 일정 방향으로 이동
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌 시
        if (other.CompareTag("Player"))
        {
            // 플레이어의 방어 스크립트를 가져와 데미지 계산
            PlayerDefense defense = other.GetComponent<PlayerDefense>();
            int finalDamage = damage;

            if (defense != null)
            {
                //finalDamage = defense.CalculateDamage(damage);
            }

            // 플레이어의 데미지 처리 함수를 호출 (예: "TakeDamage" 함수)
            other.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);

            // 투사체 제거
            Destroy(gameObject);
        }
    }
}