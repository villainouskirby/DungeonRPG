using UnityEngine;



public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    [Header("플레이어 기본 스탯")]
    public float baseAtk = 100f;
    public float baseSpeed = 5f;

    // 실제 게임에서 사용하는 “현재 스탯”
    // (버프 영향을 받은 값)
    public float currentAtk;
    public float currentSpeed;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 시작할 때 현재 스탯을 기본 스탯으로 초기화
        currentAtk = baseAtk;
        currentSpeed = baseSpeed;
    }

    // 버프가 생길 때 호출: 플레이어의 현재 스탯을 바로 변경
    public void ApplyBuff(BuffType type, float percentage)
    {
        switch (type)
        {
            case BuffType.AttackUp:
                currentAtk += baseAtk * percentage;
                break;

            case BuffType.AttackDown:
                currentAtk -= baseAtk * percentage;
                break;

            case BuffType.SpeedUp:
                currentSpeed += baseSpeed * percentage;
                break;

            case BuffType.SpeedDown:
                currentSpeed -= baseSpeed * percentage;
                break;
        }
    }

    // 버프가 끝날 때 호출: ApplyBuff에서 더해줬던 만큼 빼주거나, 빼줬던 만큼 더해서 원상 복구
    // 버프를 중간에 해제당했을 때의 로직은... 그냥 디버프식으로 덮어씌우는방식을 사용해야할지
    public void RemoveBuff(BuffType type, float percentage)
    {
        switch (type)
        {
            case BuffType.AttackUp:
                currentAtk -= baseAtk * percentage;
                break;

            case BuffType.AttackDown:
                currentAtk += baseAtk * percentage;
                break;

            case BuffType.SpeedUp:
                currentSpeed -= baseSpeed * percentage;
                break;

            case BuffType.SpeedDown:
                currentSpeed += baseSpeed * percentage;
                break;
        }
    }
    public void GetCurrentState()
    {
        //여기서 현재 스탯 값 얻기
    }
}

/*
public class Player
{
    public float Atk;
    public float Speed;
}
public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    private void Awake()
    {
        instance = this;
    }
    public Player player;
    public float originalAtk = 100;
    public float originalSpeed = 5;
    void Start()
    {
        player.Atk = originalAtk;
        player.Speed = originalSpeed;
    }

    public List<BaseBuff> onBuff = new List<BaseBuff>();

    // 이런식으로 값이 보정된 값은 따로 만들어두고
    public float BuffAtk;

    // 버프 효과가 적용된 값을 얻는 건 좋아보이는데
    // 이러면 값을 확인할려고 할때마다 Buff들을 전부 탐색해서
    // 낭비가 좀 심할거같네요
    // 버프가 추가/제거 될때마다 해당 값을 변경하는 식으로 제작하면
    // 괜찮을거같네요

    public float BuffChange(string type, float origin)
    {
        if (onBuff.Count > 0)
        {
            float temp = 0;
            for (int i = 0; i < onBuff.Count; i++) 
            {
                if (onBuff[i].type.Equals(type))
                    temp += origin * onBuff[i].percentage;
            }
            return origin + temp;
        }
        else
        {
            return origin;
        }
    }
    public void ChooseBuff(string type) 
    {
        BuffType ex = BuffType.AttackDown;
        switch(type)
        {
            case "Atk":
                player.Atk = BuffChange(type, originalAtk); break;
            case "Speed":
                player.Speed = BuffChange(type, originalSpeed); break;
        }

        switch (ex)
        {
            // 아까 얘기하던거 연장선인데 이러면 유지보수가 훨씬 쉽죠
            case BuffType.AttackUp:
                player.Atk = BuffChange(type, originalAtk); break;
            case BuffType.AttackDown:
                player.Speed = BuffChange(type, originalSpeed); break;
        }
    }
}
*/