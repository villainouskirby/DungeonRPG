using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueStatement { }

[Serializable]
public class CompoundStatement : DialogueStatement
{
    public List<DialogueStatement> Statements = new();
}

[Serializable]
public class DeclarationStatement : DialogueStatement
{
    public List<DialogueVariable.DialogueKeyValuePair> Declarations;
}

[Serializable]
public class ExprStatement : DialogueStatement
{
    public DialogueVariable Expression;
}

[Serializable]
public class DialogueLineStatement : DialogueStatement
{
    public string Speaker;
    public string Text;
}

[Serializable]
public class SelectStatement : DialogueStatement
{
    [Serializable]
    public class Choice
    {
        public string Text;
        [SerializeReference] public DialogueStatement TargetStatement;
    }

    public List<Choice> Choices = new();
}

[Serializable]
public class IfStatement : DialogueStatement
{
    public string Condition;
    [SerializeReference] public DialogueStatement ThenStatements = new();
    [SerializeReference] public DialogueStatement ElseStatements = new();
}

[Serializable]
public class JumpStatement : DialogueStatement
{
    public string Target;
}
