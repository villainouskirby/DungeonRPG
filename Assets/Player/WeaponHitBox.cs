using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    // 공격 데미지를 외부(WeaponController)에서 세팅해줌
    private int damage;

    // 충돌 시 데미지를 주기 위한 세터
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    // OnTriggerEnter2D로 충돌 판정
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적 오브젝트(Enemy 스크립트가 붙은)라면
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 데미지 적용
            enemy.TakeDamage(damage);
        }
    }
}