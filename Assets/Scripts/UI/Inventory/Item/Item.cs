using System;
using UnityEngine;

[System.Serializable]
public abstract class Item
{
    public ItemData Data { get { return _data; } private set { _data = value; } }
    [SerializeField]
    [SerializeReference]
    protected ItemData _data;
    public Item(ItemData data) => _data = data;

    public abstract Item Clone(); // TODO => 템들 이동방식 구현하면서 수정해야 할듯

    public Item()
    {

    }
}
