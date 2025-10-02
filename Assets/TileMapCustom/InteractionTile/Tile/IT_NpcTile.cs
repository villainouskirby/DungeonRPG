using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IT_NpcTile : InteractionTile
{
    [Header("Npc Setings")]
    public NPC NpcName;
}

public enum NPC
{
    Smith = 1,
    Shop = 2,
    C = 3,
}