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
    smith = 1,
    merchant = 2,
    C = 3,
    priest = 4,
}