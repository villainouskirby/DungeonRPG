using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionObjData : ScriptableObject
{
    [SerializeReference]
    public List<InteractionObj> Interaction;
}
