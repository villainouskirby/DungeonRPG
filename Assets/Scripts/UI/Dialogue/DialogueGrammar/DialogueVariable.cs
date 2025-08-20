using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public abstract class DialogueVariable
{
    [Serializable]
    public class DialogueKeyValuePair
    {
        public string Key;
        [SerializeReference] public DialogueVariable Value;

        public DialogueKeyValuePair Clone()
        {
            DialogueKeyValuePair clone = new()
            {
                Key = Key,
                Value = Value
            };

            return clone;
        }
    }

    public enum DialogueValueType
    {
        None,
        Num,
        Bool,
        String,
        Sprite,
        Image,
        Identifier
    }

    public abstract DialogueValueType GetValueType();
}

[Serializable]
public class NumVariable : DialogueVariable
{
    public float Value;
    public override DialogueValueType GetValueType() => DialogueValueType.Num;
}

[Serializable]
public class BoolVariable : DialogueVariable
{
    public bool Value;
    public override DialogueValueType GetValueType() => DialogueValueType.Bool;
}

[Serializable]
public class StringVariable : DialogueVariable
{
    public string Value;
    public override DialogueValueType GetValueType() => DialogueValueType.String;
}

// Declaration Only
[Serializable]
public class SpriteVariable : DialogueVariable
{
    public string Value;
    public override DialogueValueType GetValueType() => DialogueValueType.Sprite;
}

// Declaration Only
[Serializable]
public class ImageVariable : DialogueVariable
{
    public string Value;
    public override DialogueValueType GetValueType() => DialogueValueType.Image;
}

[Serializable]
public class ListVariable : DialogueVariable
{
    public DialogueValueType Type;
    public List<DialogueVariable> Value;
    public override DialogueValueType GetValueType() => Type;
}

[Serializable]
public class IdentifierVariable : DialogueVariable
{
    public DialogueValueType Type;
    public string ID;
    public override DialogueValueType GetValueType() => Type;
}