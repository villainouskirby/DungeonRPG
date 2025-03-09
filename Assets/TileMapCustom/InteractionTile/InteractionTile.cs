using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractionTile : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionEnum Type;
}
