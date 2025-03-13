using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public abstract class FarmableBase : MonoBehaviour
{
    public FarmEnum Type;
    public int Level;
    public List<ItemData> DropItemList;
}
