using UnityEngine;

public enum MobTriggerType
{
    Detection = 0,
    Combat = 1      
}

public class TriggerDetector : MonoBehaviour
{
    [Header("트리거 구분 Detection/Combat")]
    public MobTriggerType triggerType;

    private NormalMob parentMob;

    private void Awake()
    {
        // 부모(또는 상위)에서 NormalMob 찾기
        parentMob = GetComponentInParent<NormalMob>();
        if (!parentMob)
        {
            Debug.LogError($"부모에 NormalMob이 없습니다. [{gameObject.name}]");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!parentMob) return;

        switch (triggerType)
        {
            case MobTriggerType.Detection:
                if (other.CompareTag("PlayerSound"))
                {
                    Debug.Log($"PlayerSound 감지 Detection 상태.");
                    //parentMob.OnPlayerSoundEnter();
                }
                break;

            case MobTriggerType.Combat:
                if (other.CompareTag("Player"))
                {
                    Debug.Log($"Player 감지 Combat 상태.");
                    //parentMob.OnPlayerEnter();
                }
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!parentMob) return;

        switch (triggerType)
        {
            case MobTriggerType.Detection:
                if (other.CompareTag("PlayerSound"))
                {
                    Debug.Log($"PlayerSound 감지 해제 Detection 상태 해제.");
                    //parentMob.OnPlayerSoundExit();
                }
                break;

            case MobTriggerType.Combat:
                if (other.CompareTag("Player"))
                {
                    Debug.Log($"Player 감지 해제 Combat 상태 해제.");
                    //parentMob.OnPlayerExit();
                }
                break;
        }
    }
}