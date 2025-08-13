using UnityEngine;

[System.Serializable]
public class DialogueExpression
{
    public enum OperatorType // 기본적으로 2개의 파라미터를 가짐
    {
        None, // 파라미터 수 1개
        Get, // 파라미터 수 1개 => 현재 변수 풀에서 값 가져오기
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Pow,
        Minus, // 파라미터 수 1개
        Not, // 파라미터 수 1개
        G,
        GE,
        L,
        LE,
        E,
        NE,
        And,
        Or,
        Assign,
        AddAssgn,
        SubAssign,
        MulAssign,
        DivAssign,
        ModAssign
    }

    public OperatorType Operator;
    [UnityEngine.SerializeReference] public DialogueExpression Value1;
    [UnityEngine.SerializeReference] public DialogueExpression Value2;

    public object GetValue() => GetDialogueValue().GetValue();

    /*public DialogueValue SetValue()
    {
        switch (type)
        {
            case NumValue:
                resultValue = new NumValue();
                break;

            case BoolValue:
                resultValue = new BoolValue();
                break;

            case StringValue:
                resultValue = new StringValue();
                break;

            case SpriteValue:
                resultValue = new SpriteValue();
                break;

            case ImageValue:
                resultValue = new ImageValue();
                break;
        }
    }*/

    public DialogueValue GetDialogueValue()
    {
        DialogueValue resultValue;

        var val1 = Value1.GetValue();
        var val2 = Value2?.GetValue();

        var varType = val1.GetType();

        if (varType != val2.GetType())
        {
            Debug.LogError("Value 타입이 맞지 않음");

            return null;
        }

        
        
        if (varType == typeof(string))
        {
            switch (Operator)
            {
                case OperatorType.Add:
                    string val = (val1 as string) + (val2 as string);
                    
                    switch (val1)
                    {
                        case StringValue:
                            resultValue = new StringValue();
                            (resultValue as StringValue).Value = val;
                            break;

                        case SpriteValue:
                            resultValue = new SpriteValue();
                            (resultValue as SpriteValue).Value = val;
                            break;

                        case ImageValue:
                            resultValue = new ImageValue();
                            (resultValue as ImageValue).Value = val;
                            break;

                        default:
                            return null;
                    }
                    break;

                case OperatorType.Assign:
                    resultValue = new BoolValue();
                    (resultValue as BoolValue).Value = (val1 == val2);
                    break;

                default:
                    Debug.LogError("연산자 오류");
                    return null;
            }

            return resultValue;
        }
        else if (varType == typeof(bool))
        {
            switch (Operator)
            {

            }
        }


        switch (Operator)
        {
            case OperatorType.Add:
                break;
            case OperatorType.Sub:
                break;
            case OperatorType.Mul:
                break;
            case OperatorType.Div:
                break;
            case OperatorType.Mod:
                break;
            case OperatorType.Pow:
                break;
            case OperatorType.Minus:
                break;
            case OperatorType.Not:
                break;
            case OperatorType.G:
                break;
            case OperatorType.GE:
                break;
            case OperatorType.L:
                break;
            case OperatorType.LE:
                break;
            case OperatorType.E:
                break;
            case OperatorType.NE:
                break;
            case OperatorType.And:
                break;
            case OperatorType.Or:
                break;
            case OperatorType.Assign:
                break;
            case OperatorType.AddAssgn:
                break;
            case OperatorType.SubAssign:
                break;
            case OperatorType.MulAssign:
                break;
            case OperatorType.DivAssign:
                break;
            case OperatorType.ModAssign:
                break;
        }

        return null;
    }
}