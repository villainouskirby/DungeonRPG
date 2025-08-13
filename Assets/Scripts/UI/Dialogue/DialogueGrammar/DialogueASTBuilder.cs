using UnityEngine;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

[System.Serializable]
public class DialogueNode
{
    public string Title;
    [SerializeReference] public DialogueStatement Statement;
}

public class DialogueASTBuilder : DialogueGrammarBaseListener
{
    private List<DialogueNode> _nodes = new();
    private List<DialogueValue.DialogueKeyValuePair> _scirptValues = new();
    private DialogueNode _currentNode;
    private Stack<DialogueStatement> _statementStack = new();
    private DialogueStatement _currentStatement;
    private DialogueValue.DialogueKeyValuePair _currentValueInfo = new();

    private DialogueValue.DialogueValueType _currentValueType;

    private void PushStatement() => _statementStack.Push(_currentStatement);

    private DialogueValue GetRawDialogueValue()
    {
        return _currentValueType switch
        {
            DialogueValue.DialogueValueType.Num => new NumValue(),
            DialogueValue.DialogueValueType.Bool => new BoolValue(),
            DialogueValue.DialogueValueType.String => new StringValue(),
            DialogueValue.DialogueValueType.Sprite => new SpriteValue(),
            DialogueValue.DialogueValueType.Image => new ImageValue(),
            _ => null
        };
    }

    // 초기화
    public override void EnterScript([NotNull] DialogueGrammarParser.ScriptContext context)
    {
        _nodes.Clear();
    }

    public override void ExitScript([NotNull] DialogueGrammarParser.ScriptContext context)
    {
        // Nodes 저장
    }

    public override void EnterTitleBlock([NotNull] DialogueGrammarParser.TitleBlockContext context)
    {
        _currentNode = new();
    }

    public override void ExitTitleBlock([NotNull] DialogueGrammarParser.TitleBlockContext context)
    {
        if (_statementStack.Count > 0)
        {
            Debug.LogError("statement stack 남아있음! 문법구조 오류!");
            return;
        }

        _currentNode.Title = context.IDENTIFIER().GetText();
        _currentNode.Statement = _currentStatement;

        _nodes.Add(_currentNode);

        _currentNode = null;
        _currentStatement = null;
    }

    // =========================
    // Statements
    // =========================

    public override void EnterCompoundStatement([NotNull] DialogueGrammarParser.CompoundStatementContext context)
    {
        _currentStatement = new CompoundStatement();
        PushStatement();   
    }

    public override void EnterDeclaration([NotNull] DialogueGrammarParser.DeclarationContext context)
    {
        _currentStatement = new DeclarationStatement();
        PushStatement();

        _currentValueType = context.type().GetText() switch
        {
            "num" => DialogueValue.DialogueValueType.Num,
            "bool" => DialogueValue.DialogueValueType.Bool,
            "string" => DialogueValue.DialogueValueType.String,
            "Sprite" => DialogueValue.DialogueValueType.Sprite,
            "Image" => DialogueValue.DialogueValueType.Image,
            _ => DialogueValue.DialogueValueType.None
        };

        if (_currentValueType == DialogueValue.DialogueValueType.None)
        {
            Debug.LogError("Value Type Error");
            return;
        }
    }

    public override void EnterExprStatement([NotNull] DialogueGrammarParser.ExprStatementContext context)
    {
        _currentStatement = new ExprStatement();
        PushStatement();
    }

    public override void EnterDialogueStatement([NotNull] DialogueGrammarParser.DialogueStatementContext context)
    {
        _currentStatement = new DialogueLineStatement();
        PushStatement();
    }

    public override void EnterSelectStatement([NotNull] DialogueGrammarParser.SelectStatementContext context)
    {
        _currentStatement = new SelectStatement();
        PushStatement();
    }

    public override void EnterIfStatement([NotNull] DialogueGrammarParser.IfStatementContext context)
    {
        _currentStatement = new IfStatement();
        PushStatement();
    }

    public override void EnterJumpStatement([NotNull] DialogueGrammarParser.JumpStatementContext context)
    {
        _currentStatement = new JumpStatement();
        PushStatement();
    }

    public override void ExitStatement([NotNull] DialogueGrammarParser.StatementContext context)
    {
        DialogueStatement prevStatement = _statementStack.Pop();
        _currentStatement = _statementStack.Peek();
        
        switch (_currentStatement)
        {
            case CompoundStatement:
                (_currentStatement as CompoundStatement).Statements.Add(prevStatement);
                break;

            case SelectStatement:
                break;

            case IfStatement:
                break;

            case JumpStatement:
                break;
        }
    }

    // =========================
    // Declaration
    // =========================

    public override void EnterInitDeclarator([NotNull] DialogueGrammarParser.InitDeclaratorContext context)
    {
        if (_currentValueType == DialogueValue.DialogueValueType.None) return;

        _currentValueInfo.Key = context.declarator().IDENTIFIER().GetText();

        DialogueGrammarParser.InitializerContext targetInitializer;
        
    }

    // =========================
    // Expressions
    // =========================


}