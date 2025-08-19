using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public abstract class DialogueValue
{
    [Serializable]
    public class DialogueKeyValuePair
    {
        public string Key;
        [SerializeReference] public DialogueValue Value;

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
        Image
    }

    public abstract object GetValue();
}

[Serializable]
public class NumValue : DialogueValue
{
    public float Value;
    public override object GetValue() => Value;
}

[Serializable]
public class BoolValue : DialogueValue
{
    public bool Value;
    public override object GetValue() => Value;
}

[Serializable]
public class StringValue : DialogueValue
{
    public string Value;
    public override object GetValue() => Value;
}

[Serializable]
public class SpriteValue : DialogueValue
{
    public string Value;
    public override object GetValue() => Value;
}

[Serializable]
public class ImageValue : DialogueValue
{
    public string Value;
    public override object GetValue() => Value;
}

[Serializable]
public class ListValue : DialogueValue
{
    public List<DialogueValue> Value;
    public override object GetValue() => Value;
}