using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeAnalysisApp1
{
    public class DoubleToSingleRewriter : CSharpSyntaxRewriter
    {

        public int Count { get;private set;}

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.NumericLiteralExpression) && node.Token.Value is double)
            {
                return LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal((float)(double) node.Token.Value)
                );
            }
            return base.VisitLiteralExpression(node);
        }

    }
}
