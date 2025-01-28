using UnityEngine;

public abstract class BattleEquipmentItemData : EquipmentItemData
{
    public int Damage => _damage;
    public int Hp => _hp;
    public int Stamina => _stamina;
    public int Duration => _duration;
    public int AdditionalEffect => _additionalEffect;

    [SerializeField] private int _damage;
    [SerializeField] private int _hp;
    [SerializeField] private int _stamina;
    [SerializeField] private int _duration;
    [SerializeField] private int _additionalEffect;
}
