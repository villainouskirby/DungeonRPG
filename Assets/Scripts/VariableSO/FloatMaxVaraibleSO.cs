using System.Collections.Generic;
using UnityEngine;

public class FloatMaxVaraibleSO : VariableSO<float>
{
    [SerializeField] private float _maxValue;

    public float MaxValue { get => _maxValue; set { _maxValue = value; } }

    public override float Value 
    {
        set
        {
            if (!EqualityComparer<float>.Default.Equals(base.Value, value))
            {
                base.Value = Mathf.Min(base.Value + value, _maxValue);
            }
        }
    }
}
