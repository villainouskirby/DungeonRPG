using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IT_TeleportObj : InteractionObj
{
    public Vector3 TargetPos;

    public IT_TeleportObj(InteractionTile interactionTile) : base(interactionTile)
    {
    }

    public IT_TeleportObj()
    {

    }
}
