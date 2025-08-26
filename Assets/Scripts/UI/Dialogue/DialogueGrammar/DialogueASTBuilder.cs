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
    private DialogueNode _currentNode;

    private List<DialogueVariable.DialogueKeyValuePair> _scirptVariables = new();

    private Stack<DialogueStatement> _statementStack = new();
    private DialogueStatement _currentStatement;

    private Stack<DialogueVariable> _variableValueStack;
    private DialogueVariable.DialogueKeyValuePair _currentVariableInfo = new();
    private DialogueVariable.DialogueValueType _currentValueType;

    private void PushStatement() => _statementStack.Push(_currentStatement);

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

    public override void EnterStatement([NotNull] DialogueGrammarParser.StatementContext context)
    {
        if (_currentStatement != null)
        {
            PushStatement();
        }
    }

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
            "num" => DialogueVariable.DialogueValueType.Num,
            "bool" => DialogueVariable.DialogueValueType.Bool,
            "string" => DialogueVariable.DialogueValueType.String,
            "Sprite" => DialogueVariable.DialogueValueType.Sprite,
            "Image" => DialogueVariable.DialogueValueType.Image,
            _ => DialogueVariable.DialogueValueType.None
        };

        if (_currentValueType == DialogueVariable.DialogueValueType.None)
        {
            Debug.LogError("Value Type Error");
            return;
        }
    }

    public override void EnterExprStatement([NotNull] DialogueGrammarParser.ExprStatementContext context)
    {
        _currentStatement = new ExprStatement();
    }

    public override void EnterDialogueStatement([NotNull] DialogueGrammarParser.DialogueStatementContext context)
    {
        _currentStatement = new DialogueLineStatement();
    }

    public override void EnterSelectStatement([NotNull] DialogueGrammarParser.SelectStatementContext context)
    {
        _currentStatement = new SelectStatement();
    }

    public override void EnterIfStatement([NotNull] DialogueGrammarParser.IfStatementContext context)
    {
        _currentStatement = new IfStatement();
    }

    public override void EnterJumpStatement([NotNull] DialogueGrammarParser.JumpStatementContext context)
    {
        _currentStatement = new JumpStatement();
    }

    public override void ExitStatement([NotNull] DialogueGrammarParser.StatementContext context)
    {
        if (_statementStack.Count == 0)
        {
            return;
        }

        DialogueStatement prevStatement = _currentStatement;

        _currentStatement = _statementStack.Pop();
        
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
        if (_currentValueType == DialogueVariable.DialogueValueType.None) return;

        _currentVariableInfo.Key = context.declarator().IDENTIFIER().GetText();

        DialogueGrammarParser.InitializerContext targetInitializer = context.initializer();

    }

    // =========================
    // Expressions
    // =========================

    public override void EnterExpr([NotNull] DialogueGrammarParser.ExprContext context)
    {
        base.EnterExpr(context);
    }

    public override void EnterAssignExpr([NotNull] DialogueGrammarParser.AssignExprContext context)
    {
        base.EnterAssignExpr(context);
    }

    public override void EnterOrExpr([NotNull] DialogueGrammarParser.OrExprContext context)
    {
        base.EnterOrExpr(context);
    }

    public override void EnterAndExpr([NotNull] DialogueGrammarParser.AndExprContext context)
    {
        base.EnterAndExpr(context);
    }

    public override void EnterEqualityExpr([NotNull] DialogueGrammarParser.EqualityExprContext context)
    {
        base.EnterEqualityExpr(context);
    }

    public override void EnterRelationalExpr([NotNull] DialogueGrammarParser.RelationalExprContext context)
    {
        base.EnterRelationalExpr(context);
    }

    public override void EnterAdditiveExpr([NotNull] DialogueGrammarParser.AdditiveExprContext context)
    {
        base.EnterAdditiveExpr(context);
    }

    public override void EnterMultiplicativeExpr([NotNull] DialogueGrammarParser.MultiplicativeExprContext context)
    {
        base.EnterMultiplicativeExpr(context);
    }

    public override void EnterPowerExpr([NotNull] DialogueGrammarParser.PowerExprContext context)
    {
        base.EnterPowerExpr(context);
    }


    public override void EnterUnaryExpr([NotNull] DialogueGrammarParser.UnaryExprContext context)
    {
        base.EnterUnaryExpr(context);
    }

    public override void EnterPostfixExpr([NotNull] DialogueGrammarParser.PostfixExprContext context)
    {
        base.EnterPostfixExpr(context);
    }

    public override void EnterPrimaryExpr([NotNull] DialogueGrammarParser.PrimaryExprContext context)
    {
        if (context.value() != null)
        {
            DialogueVariable valriable = _currentValueType switch
            {
                DialogueVariable.DialogueValueType.Num => new NumVariable(),
                DialogueVariable.DialogueValueType.Bool => new BoolVariable(),
                DialogueVariable.DialogueValueType.String => new StringVariable(),
                DialogueVariable.DialogueValueType.Sprite => new SpriteVariable(),
                DialogueVariable.DialogueValueType.Image => new ImageVariable(),
                DialogueVariable.DialogueValueType.Identifier => new IdentifierVariable(),
                _ => null
            };
        }
    }
}