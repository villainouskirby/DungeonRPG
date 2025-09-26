using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new DialogueSO", menuName = "DialogueScript")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueLineStatement> Lines;
    public string EndEventKey;
}