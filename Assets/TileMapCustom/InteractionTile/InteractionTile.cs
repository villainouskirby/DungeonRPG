using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractionTile : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionEnum Type;
    public float Z;
    public string LayerName;
    public int LayerIndex;
    public Vector2 ColliderBounds;
    public Vector2 ColliderOffset;
}
