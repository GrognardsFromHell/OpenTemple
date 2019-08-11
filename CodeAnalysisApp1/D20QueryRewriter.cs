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

        private static SyntaxNode ReplaceD20QueryComparison(SyntaxNode node, InvocationExpressionSyntax invocation, LiteralExpressionSyntax literal)
        {

            var numericLiteral = (int) literal.Token.Value;

            if (numericLiteral == 0 && node.IsKind(SyntaxKind.NotEqualsExpression)
                        || numericLiteral == 1 && node.IsKind(SyntaxKind.EqualsExpression))
            {
                // != 0 -> wants it to be true
                return invocation;
            }
            else if (numericLiteral == 1 && node.IsKind(SyntaxKind.NotEqualsExpression)
                || numericLiteral == 0 && node.IsKind(SyntaxKind.EqualsExpression))
            {
                // != 1
                return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, invocation);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
                return node;
            }
        }

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            // One side of the expression must be a call to D20Query

            if (node.IsKind(SyntaxKind.NotEqualsExpression) || node.IsKind(SyntaxKind.EqualsExpression))
            {
                if (node.Left is InvocationExpressionSyntax invocation && invocation.Expression.ToFullString() == "GameSystems.D20.D20Query"
                    && node.Right is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    return ReplaceD20QueryComparison(node, invocation, literal);
                }
                else if (node.Right is InvocationExpressionSyntax invocation2 && invocation2.Expression.ToFullString() == "GameSystems.D20.D20Query"
                    && node.Left is LiteralExpressionSyntax literal2 && literal2.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    return ReplaceD20QueryComparison(node, invocation2, literal2);
                }
            }

            return base.VisitBinaryExpression(node);
        }
    }
}
