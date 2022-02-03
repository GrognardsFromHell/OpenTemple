using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGenerator;

public class MethodArgumentVisitor : CSharpSyntaxWalker
{
    private readonly SemanticModel _semanticModel;

    public string ConditionName { get; private set; }

    public int NumArgs { get; private set; }

    private int _argCount;

    public MethodArgumentVisitor(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public override void VisitArgument(ArgumentSyntax node)
    {
        var argType = _semanticModel.GetTypeInfo(node.Expression);

        if (_argCount == 0)
        {
            // First argument should be the condition name
            if (node.Expression is LiteralExpressionSyntax literal && literal.Token.Value is string condName)
            {
                ConditionName = condName;
            }
        }

        base.VisitArgument(node);

        _argCount++;
    }
}