using System;
using UnityEngine;

/// <summary> 셀 수 있는 아이템 </summary>
public abstract class CountableItem : Item
{
    public CountableItemData CountableData => Data as CountableItemData;

    /// <summary> 현재 아이템 개수 </summary>
    public int Amount;
    /// <summary> 하나의 슬롯당 최대 개수(기본값 : 99) </summary>
    public int MaxAmount => CountableData.MaxAmount;
    /// <summary> 수량이 가득 찼는지 여부 </summary>
    public bool IsMax => Amount >= CountableData.MaxAmount;
    /// <summary> 개수가 없는지 여부 </summary>
    public bool IsEmpty => Amount <= 0;

    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        SetAmount(amount);
    }

    /// <summary> 개수 지정(범위 제한)</summary>
    public void SetAmount(int amount)
    {
        Amount = Math.Clamp(amount, 0, MaxAmount);
    }

    /// <summary> 개수 추가 </summary>
    /// <returns> 최대치 초과량(없으면 0) </returns>
    public int AddAmountAndGetExcess(int amount)
    {
        int nextAmount = Amount + amount;
        SetAmount(nextAmount);

        return (nextAmount > MaxAmount) ? (nextAmount - MaxAmount) : 0;
    }

    /// <summary> 개수 나누어 복제 </summary>
    /// <returns> 분리되어 복제된 객체 </returns>
    public CountableItem SeperateClone(int amount)
    {
        if (amount <= 1) return null; // 1개면 분리 불가

        if (amount > Amount - 1) amount = Amount - 1; // 기존의 것이 최소한 1개는 남도록 수정

        Amount -= amount;
        return (CountableItem)Clone(amount);
    }

    public override Item Clone() => Clone(1);

    public abstract Item Clone(int amount);
}
