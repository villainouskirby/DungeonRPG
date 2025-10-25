using System;
using System.Collections.Generic;
using UnityEngine;

public class PotionItemData : CountableItemData
{
    public enum PotionType
    {
        None,
        Heal,
        Add,
        Remove
    }

    public int EffectAmount => _potionInfo.effect;
    public float EffectDuration => _potionInfo.duration;
    public PotionType EffectType => _effectType;
    public string EffectName => _potionInfo.buff;


    [SerializeField] private PotionType _effectType;

    public Item_Info_Potion PotionInfo => _potionInfo;

    [SerializeReference] private Item_Info_Potion _potionInfo;

    // public Dictionary<string, Item_Info_Item> PotionDic;
    public PotionItemData(Item_Info_Item info) : base(info)
    {
        _potionInfo = Array.Find(Item_Info.Potion, potion => potion.id == info.id);
        _effectType = _potionInfo.type switch
        {
            "heal" => PotionType.Heal,
            "add" => PotionType.Add,
            "remove" => PotionType.Remove,
            _ => PotionType.None
        };
    }

    public override Item Createitem()
    {
        return new PotionItem(this);
    }
}
