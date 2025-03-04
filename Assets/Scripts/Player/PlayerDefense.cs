using UnityEngine;

public class PlayerDefense : MonoBehaviour
{
    public float justGuardWindow = 0.3f; // 저스트 가드 허용 시간
    private float lastGuardTime = -1f;     // 마지막 방어 입력 시간

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastGuardTime = Time.time;
            // 여기서 방어 애니메이션/이펙트 트리거 가능
        }
    }

    // 투사체 충돌 시 호출하여 최종 데미지를 계산하는 함수
    public int CalculateDamage(int incomingDamage)
    {
        float timeSinceGuard = Time.time - lastGuardTime;

        if (timeSinceGuard <= justGuardWindow)
        {
            // 저스트 가드: 아주 정확한 타이밍에 방어하면 데미지를 완전히 막거나 특별한 효과를 줄 수 있음
            Debug.Log("Just Guard activated!");
            return 0;
        }
        else if (Input.GetMouseButton(1))
        {
            // 방어 중인 상태에서, 저스트 가드 시간보다 늦게 입력된 경우 → 70% 데미지 감쇄 (즉, 30%만 받음)
            Debug.Log("Guarding: Damage reduced by 70%");
            return Mathf.RoundToInt(incomingDamage * 0.3f);
        }
        else
        {
            // 방어하지 않은 상태이면 원래 데미지 그대로 적용
            return incomingDamage;
        }
    }
}
