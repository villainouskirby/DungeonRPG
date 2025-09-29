using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IT_EntryObj : InteractionObj
{
    public MapEnum MapType;
    public int Layer;

    public IT_EntryObj(InteractionTile interactionTile) : base(interactionTile)
    {
    }

    public IT_EntryObj()
    {

    }
}
