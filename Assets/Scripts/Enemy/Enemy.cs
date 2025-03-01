using UnityEngine;
using TMPro; // TextMeshPro를 사용하려면 필요

public class Enemy : MonoBehaviour
{
    [Header("HP 설정")]
    public int maxHP = 300;
    private int currentHP;

    [Header("UI 설정")]
    public TextMeshProUGUI hpText; // 씬에서 할당

    private void Start()
    {
        currentHP = maxHP;
        UpdateHPText();
    }

    // WeaponHitbox로부터 데미지를 받으면 실행
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateHPText();

        // HP가 0 이하가 되면 사망 처리
        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 여기서는 간단히 오브젝트 파괴
        Destroy(gameObject);
    }

    private void UpdateHPText()
    {
        if (hpText != null)
        {
            hpText.text = "HP: " + currentHP;
        }
    }
}