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
    A = 1,
    B = 2,
    C = 3,
}