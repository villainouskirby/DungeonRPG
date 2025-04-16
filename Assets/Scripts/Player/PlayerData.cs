using UnityEngine;



public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    [Header("Base Stats (기본 스탯 값)")]
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseSpeed = 10f;

    [Header("Max Stats (스탯 최대 값)")]
    [SerializeField] private float MaxHP = 100f;
    [SerializeField] private float MaxStamina = 100f;

    [Header("Current Stats (게임 중 변동)")]
    [SerializeField] private FloatVariableSO currentAtk;
    [SerializeField] private FloatVariableSO currentSpeed;
    [SerializeField] private FloatVariableSO currentHP;
    [SerializeField] private FloatVariableSO currentStamina;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
    }
    // 버프가 생길 때 호출: 플레이어의 현재 스탯을 바로 변경
    public void ApplyBuff(BuffType type, float percentage)
    {
        switch (type)
        {
            case BuffType.AttackUp:
                currentAtk.Value += baseAttack * percentage;
                break;

            case BuffType.AttackDown:
                currentAtk.Value -= baseAttack * percentage;
                break;

            case BuffType.SpeedUp:
                currentSpeed.Value += baseSpeed * percentage;
                break;

            case BuffType.SpeedDown:
                currentSpeed.Value -= baseSpeed * percentage;
                break;
        }
    }
    public void HPValueChange(float value)
    {
        currentHP.Value += value;
    }
    // 버프가 끝날 때 호출: ApplyBuff에서 적용했던 값 반대로 적용하기
    public void RemoveBuff(BuffType type, float percentage)
    {
        // ApplyBuff와 반대로 돌려주면 됨
        ApplyBuff(Opposite(type), percentage);
    }

    private BuffType Opposite(BuffType t) => t switch
    {
        BuffType.AttackUp => BuffType.AttackDown,
        BuffType.AttackDown => BuffType.AttackUp,
        BuffType.SpeedUp => BuffType.SpeedDown,
        BuffType.SpeedDown => BuffType.SpeedUp,
        _ => t
    };
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