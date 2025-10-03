using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IT_JumpObj : InteractionObj
{
    public int TargetLayer;
    public int TargetGround;
    public int TargetHeight;

    public IT_JumpObj(InteractionTile interactionTile) : base(interactionTile)
    {
    }

    public IT_JumpObj()
    {

    }
}
