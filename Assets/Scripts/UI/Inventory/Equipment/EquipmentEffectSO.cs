using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentEffect", menuName = "PlayerData/EquipmentEffect")]
public class EquipmentEffectSO : ScriptableObject
{
    public int Damage;
    public int Hp;
    public int Stamina;
    public List<int> AdditionalEffects = new List<int>(); // TODO => 부과효과 기획 나온 후 확립
}
