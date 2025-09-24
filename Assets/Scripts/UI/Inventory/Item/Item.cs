using System;
using UnityEngine;

[System.Serializable]
public abstract class Item
{
    public ItemData Data => _data;

    [SerializeField]
    [SerializeReference]
    private ItemData _data;

    public Item(ItemData data)
    {
        _data = data;
    }

    public abstract Item Clone(); // TODO => 템들 이동방식 구현하면서 수정해야 할듯
}
