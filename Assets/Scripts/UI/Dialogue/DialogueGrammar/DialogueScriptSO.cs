using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Script", menuName = "DialogueScriptSO")]
public class DialogueScriptSO : ScriptableObject
{
    public List<DialogueNode> Nodes;
    public List<DialogueValue.DialogueKeyValuePair> Values;
}