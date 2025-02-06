using System.Collections.Generic;
using UnityEngine;

public class ItemListSO<T> : ScriptableObject where T : Item
{
    [SerializeReference]
    public List<T> Items;
}
