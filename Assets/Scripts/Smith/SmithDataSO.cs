using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SmithList", menuName = "ItemList/SmithList")]
public class SmithDataSO : ScriptableObject
{
    [SerializeReference]
    public List<SmithData> SmithDatas;
}
