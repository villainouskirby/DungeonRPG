[System.Serializable]
public class DialogueExpression : DialogueVariable
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

    public DialogueValueType Type;
    public OperatorType Operator;
    [UnityEngine.SerializeReference] public DialogueVariable Value1;
    [UnityEngine.SerializeReference] public DialogueVariable Value2;

    public override DialogueValueType GetValueType() => Type;
    public bool CheckType() => Value1.GetValueType() == Value2.GetValueType();
}