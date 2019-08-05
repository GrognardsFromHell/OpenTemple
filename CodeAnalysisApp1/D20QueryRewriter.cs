using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeAnalysisApp1
{
    public class D20QueryRewriter : CSharpSyntaxRewriter
    {

        private readonly SemanticModel SemanticModel;

        public D20QueryRewriter(SemanticModel semanticModel) => SemanticModel = semanticModel;

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            // One side of the expression must be a call to D20Query

            if (node.IsKind(SyntaxKind.NotEqualsExpression))
            {
                if (node.Left is InvocationExpressionSyntax invocation && invocation.Expression.ToFullString() == "GameSystems.D20.D20Query"
                    && node.Right is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    if (literal.ToFullString() == "0")
                    {
                        // != 0 -> wants it to be true
                        return node.Left;
                    }
                    else if (literal.ToFullString() == "1")
                    {
                        // != 1
                        return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, invocation);
                    }
                }
                else if (node.Right is InvocationExpressionSyntax invocation2 && invocation2.Expression.ToFullString() == "GameSystems.D20.D20Query"
                    && node.Left is LiteralExpressionSyntax literal2 && literal2.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    if (literal2.ToFullString() == "0")
                    {
                        // != 0 -> wants it to be true
                        return node.Left;
                    }
                    else if (literal2.ToFullString() == "1")
                    {
                        // != 1
                        return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, invocation2);
                    }
                }
            }

            return base.VisitBinaryExpression(node);
        }
    }
}
