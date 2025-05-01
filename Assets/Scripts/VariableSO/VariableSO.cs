using System.Collections.Generic;
using UnityEngine;

public class VariableSO<T> : ScriptableObject
{
    [SerializeField] private T value;

    /// <summary> 값이 변하면 Invoke 된다. </summary>
    public event ValueChanged OnValueChanged;

    public virtual T Value
    {
        get => value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(this.value, value))
            {
                this.value = value;
                OnValueChanged?.Invoke(value);
            }
        }
    }


    public delegate void ValueChanged(T value);
}