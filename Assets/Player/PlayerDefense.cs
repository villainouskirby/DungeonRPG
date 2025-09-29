using UnityEngine;

/// • 가드·저스트 가드 계산 + 스태미너 차감 + 쿨타임 관리 담당
public class PlayerDefense : MonoBehaviour
{
    [Header("윈도우(↓) · 후딜(↑)")]
    [SerializeField] float justGuardWindow = 0.20f;  // 버튼 누른 뒤 허용 시간
    [SerializeField] float normalGuardDelay = 0.50f;  // 가드 성공 후 쿨다운
    [SerializeField] float justGuardDelay = 0.30f;  // 저스트 가드 후 쿨다운

    [Header("가드 성공시 얻는 데미지 비율")]
    [SerializeField] float normalGuardRatio = 0.5f;    // 50%만 받음
    [SerializeField] float justGuardRatio = 0.3f;    // 30%만 받음

    [Header("스태미너 소모")]
    [SerializeField] int normalGuardCost = 50;
    [SerializeField] int justGuardCost = 30;

    float lastGuardPress = 0f;   // 마지막으로 우클릭을 누른 시각
    float guardCooldown = 0f;      // 남은 쿨타임(초)

    void Update()
    {
        if (guardCooldown > 0f)
            guardCooldown -= Time.deltaTime;

        // 우클릭 'Down' 기록 (가드 판정용)
        if (Input.GetMouseButtonDown(1)) { lastGuardPress = Time.time; }
    }

    public int ResolveGuard(int incomingDamage)
    {
        // 가드 중? (버튼 누르고 있고, 쿨타임 없음, 스태미너 충분)
        bool holding = Input.GetMouseButton(1);
        if (!holding || guardCooldown > 0f) return incomingDamage;

        if (!PlayerData.Instance) return incomingDamage;

        // 저스트 여부 
        bool isJust = Time.time - lastGuardPress <= justGuardWindow;

        // 스태미너 확인·차감 
        int cost = isJust ? justGuardCost : normalGuardCost;
        if (!PlayerData.Instance.SpendStamina(cost))      // 부족 → 가드 실패
            return incomingDamage;

        // 감쇄 비율·쿨타임 적용
        guardCooldown = isJust ? justGuardDelay : normalGuardDelay;
        float ratio = isJust ? justGuardRatio : normalGuardRatio;

        Debug.Log(isJust ? "JUST GUARD!" : "Guard success");
        return Mathf.RoundToInt(incomingDamage * ratio);
    }

    public bool GuardAvailable => Input.GetMouseButton(1) && guardCooldown <= 0f;
}