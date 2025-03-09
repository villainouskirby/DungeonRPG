using System.Collections.Generic;
using UnityEngine;

public class Smith : MonoBehaviour
{
    [Header("Smith UI")]
    [SerializeField] private SmithUI _smithUI;

    [Header("Smith Data List")]
    [SerializeField] private SmithDataSO _armorList;
    [SerializeField] private SmithDataSO _weaponList;
    [SerializeField] private SmithDataSO _toolList;

    private List<SmithData> _armors => _armorList.SmithDatas;
    private List<SmithData> _weapons => _weaponList.SmithDatas;
    private List<SmithData> _tools => _toolList.SmithDatas;

    public SmithData GetSmithData(int index) => (_smithUI.Category switch
    {
        EquipmentType.bodyArmor => _armors,
        EquipmentType.weapon => _weapons,
        EquipmentType.tool => _tools,
        _ => _armors
    })[index];

    public void InitSmith()
    {
        _smithUI.InitSmithUI();
    }
}
