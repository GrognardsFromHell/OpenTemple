using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeAnalysisApp1
{
    public class SemanticRewriter : CSharpSyntaxRewriter
    {

        public int Count { get; private set; }
        public int VoidCallUnroll { get; private set; }

        private readonly SemanticModel SemanticModel;

        public SemanticRewriter(SemanticModel semanticModel) => SemanticModel = semanticModel;

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            // if ( GameSystems.Spell.UpdateSpellPacket(spellPkt) == 1 )
            if (node.Condition is BinaryExpressionSyntax bse
                && bse.Left is InvocationExpressionSyntax ies
                && ies.ToFullString().Contains("UpdateSpellPacket")
                && bse.Right is LiteralExpressionSyntax les
                && les.Token.ValueText == "1")
            {
                VoidCallUnroll++;
                
                BlockSyntax block = Block(ExpressionStatement(bse.Left));
                if (node.Statement is BlockSyntax bs)
                {
                    block = block.AddStatements(bs.Statements.ToArray());
                }
                else
                {
                    block = block.AddStatements(node.Statement);
                }

                if (node.Else != null)
                {
                    block = block.WithTrailingTrivia(TriviaList(Comment(
                        "/*" + node.Else.ToFullString() + "*/"
                    )));
                }

                return block.NormalizeWhitespace();
            }

            if (HasIntOrEnumType(node.Condition))
            {
                // Transform if ( X ) -> if ( X != 0 )
                var newIf = base.VisitIfStatement(node) as IfStatementSyntax;
                return newIf.WithCondition(ConvertIntToBool(newIf.Condition));
            }
            else if (HasReferenceType(node.Condition))
            {
                // Transform if ( X ) -> if ( X != null )
                var newIf = base.VisitIfStatement(node) as IfStatementSyntax;
                return newIf.WithCondition(CompareNotNull(newIf.Condition));
            }

            return base.VisitIfStatement(node);
        }

        public override SyntaxNode VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            // Translate switch statements on something ".spellEnum" to use the spell names as labels
            if (node.Parent.Parent is SwitchStatementSyntax switchNode)
            {
                if (switchNode.Expression.ToFullString().Trim().EndsWith(".spellEnum"))
                {
                    if (SpellMapping.TryMapSpellId(node.Value, out var spellRef))
                    {
                        return node.WithValue(spellRef);
                    }
                }
            }

            return base.VisitCaseSwitchLabel(node);
        }

        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            // Try to auto convert integer literals to the enums
            if (node.Expression.IsKind(SyntaxKind.NumericLiteralExpression) 
                && node.Expression is LiteralExpressionSyntax literalExpr) { 
                
                var argumentList = node.Parent as ArgumentListSyntax;
                if (argumentList == null)
                {
                    return node;
                }
                var invocation = argumentList.Parent as InvocationExpressionSyntax;
                if (invocation != null) {
                    var methodGroup = SemanticModel.GetMemberGroup(invocation.Expression).OfType<IMethodSymbol>().FirstOrDefault();
                    var expressionType = SemanticModel.GetTypeInfo(invocation.Expression).Type as INamedTypeSymbol;

                    var methodSymbol = SemanticModel
                        .GetSymbolInfo(invocation.Expression)
                        .Symbol as IMethodSymbol;
                    methodSymbol = methodSymbol ?? methodGroup; // If concrete invocation is not determinable, use the one from just the method name
                    if (methodSymbol != null)
                    {
                        var paramTypes = methodSymbol.Parameters;
                        var paramIndex = argumentList.Arguments.IndexOf(node);
                        if (paramIndex != -1 && paramIndex < paramTypes.Length)
                        {
                            var paramType = paramTypes[paramIndex];
                            
                            var mappedArg = EnumMapping.MapNumberToEnumSyntax(paramType.Type?.Name, literalExpr.Token.Value);
                            if (mappedArg != null)
                            {
                                return node.WithExpression(mappedArg);
                            }
                        }
                    }
                }
            }

            return base.VisitArgument(node);
        }

        private bool HasIntOrEnumType(SyntaxNode operand)
        {
            if (SemanticModel.SyntaxTree == operand.SyntaxTree)
            {
                var operandType = SemanticModel.GetTypeInfo(operand);

                return operandType.Type != null && EnumMapping.Enums.ContainsKey(operandType.Type.Name) 
                    || operandType.ConvertedType?.SpecialType == SpecialType.System_Int32;
            }
            return false;
        }
        private bool HasReferenceType(SyntaxNode operand)
        {
            if (SemanticModel.SyntaxTree == operand.SyntaxTree)
            {
                var operandType = SemanticModel.GetTypeInfo(operand);
                if (operandType.Type != null && operandType.Type.IsReferenceType && operandType.Type.TypeKind != TypeKind.Error)
                {
                    return true;
                }
            }
            return false;
        }
        
        private ExpressionSyntax ConvertIntToBool(ExpressionSyntax operand, bool inverted = false)
        {
            var node = operand;
            if (!(node is ParenthesizedExpressionSyntax))
            {
                node = ParenthesizedExpression(node);
            }

            if (inverted) { 
                return BinaryExpression(SyntaxKind.EqualsExpression, node, LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)));
            } else
            {
                return BinaryExpression(SyntaxKind.NotEqualsExpression, node, LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)));
            }
        }

        public override SyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            // Apply !(x != y) -> (x == y) and !(x == y) -> x != y
            var newNode = base.VisitPrefixUnaryExpression(node);

            if (newNode is PrefixUnaryExpressionSyntax unaryOp && unaryOp.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var operand = unaryOp.Operand as ExpressionSyntax;
                if (operand is ParenthesizedExpressionSyntax parens)
                {
                    operand = parens.Expression;
                }
                if (operand is BinaryExpressionSyntax bes)
                {
                    if (bes.IsKind(SyntaxKind.NotEqualsExpression))
                    {
                        return ParenthesizedExpression(bes.WithOperatorToken(Token(SyntaxKind.EqualsEqualsToken))).NormalizeWhitespace();
                    }
                    else if (bes.IsKind(SyntaxKind.EqualsExpression))
                    {
                        return ParenthesizedExpression(bes.WithOperatorToken(Token(SyntaxKind.ExclamationEqualsToken))).NormalizeWhitespace();
                    }
                }
                    
                if (HasIntOrEnumType(operand))
                {
                    return ConvertIntToBool(operand, true);
                }
                if (HasReferenceType(operand)) 
                {
                    return ParenthesizedExpression(
                        BinaryExpression(SyntaxKind.EqualsExpression, operand, LiteralExpression(SyntaxKind.NullLiteralExpression))
                    ).NormalizeWhitespace();
                }
            }

            return newNode;
        }

        private bool TryMakeEnumBitfieldCheck(SyntaxNode genericNode, out BinaryExpressionSyntax fixedNode)
        {
            fixedNode = null;
            if (!(genericNode is BinaryExpressionSyntax node)) { 
                return false;
            }

            if (node.IsKind(SyntaxKind.BitwiseAndExpression)
                && node.Right is LiteralExpressionSyntax maskLiteral)
            {
                object value = maskLiteral.Token.Value;
                //  LHS will always be an expression that has a known enum type
                var type = SemanticModel.GetTypeInfo(node.Left);
                if (EnumMapping.Enums.ContainsKey(type.Type.Name))
                {
                    var v = EnumMapping.MapNumberToEnumSyntax(type.Type.Name, value);
                    if (v != null)
                    {
                        fixedNode = BinaryExpression(
                            SyntaxKind.NotEqualsExpression,
                            // Add parenthesis
                            ParenthesizedExpression(node.WithRight(v)),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))
                        );
                        return true;
                    }
                }
            }

            return false;
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var leftType = SemanticModel.GetTypeInfo(node.Left);
            if (leftType.Type?.Name == "Stat")
            {
                return node.WithLeft(base.Visit(node.Left) as ExpressionSyntax)
                    .WithRight(
                    CastExpression(IdentifierName("Stat"), base.Visit(node.Right) as ExpressionSyntax)
                    );
            }
            else if (node.Right is LiteralExpressionSyntax literal 
                && literal.IsKind(SyntaxKind.NumericLiteralExpression)
                && EnumMapping.MapNumberToEnumSyntax(leftType.Type?.Name, literal.Token.Value) != null)
            {
                return node.WithLeft(base.Visit(node.Left) as ExpressionSyntax)
                    .WithRight(
                    EnumMapping.MapNumberToEnumSyntax(leftType.Type?.Name, literal.Token.Value)
                    );
            }

            return base.VisitAssignmentExpression(node);
        }

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (TryMakeEnumBitfieldCheck(node, out var enumBitfieldNode))
            {
                return enumBitfieldNode;
            }

            var leftType = SemanticModel.GetTypeInfo(node.Left);
            var rightType = SemanticModel.GetTypeInfo(node.Right);
            
            // Fix comparisons with Enums
            if (leftType.Type?.Name == "D20ActionType" && node.Right.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                var actionType = (int) ((LiteralExpressionSyntax) node.Right).Token.Value;
                var newBase = base.VisitBinaryExpression(node) as BinaryExpressionSyntax;
                return newBase.WithRight(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("D20ActionType"), IdentifierName(ActionMapping.Mapping[actionType]))
                );
            }

            // Quick and dumb check for spell enums
            if (node.Left.ToFullString().TrimEnd().EndsWith(".spellEnum") && node.Right.IsKind(SyntaxKind.NumericLiteralExpression))
            {
               if (SpellMapping.TryMapSpellId(node.Right, out var spellNameRef))
                {
                    return node.WithRight(spellNameRef);
                }
            }

            // Comparing bool to int sucks, we need to replace 1 with true
            if (node.IsKind(SyntaxKind.NotEqualsExpression) || node.IsKind(SyntaxKind.EqualsExpression))
            {
                // Replace the right side with a boolean expression
                {
                    if (leftType.ConvertedType?.SpecialType == SpecialType.System_Boolean
                        && node.Right is LiteralExpressionSyntax leftLiteralNode
                        && leftLiteralNode.IsKind(SyntaxKind.NumericLiteralExpression))
                    {
                        var numericLiteral = (int)leftLiteralNode.Token.Value;
                        if (numericLiteral == 1 && node.IsKind(SyntaxKind.EqualsExpression)
                            || numericLiteral == 0 && node.IsKind(SyntaxKind.NotEqualsExpression))
                        {
                            Count++;
                            return node.Left;
                        }
                        else if (numericLiteral == 0 && node.IsKind(SyntaxKind.EqualsExpression)
                                 || numericLiteral == 1 && node.IsKind(SyntaxKind.NotEqualsExpression))
                        {
                            Count++;
                            return PrefixUnaryExpression(
                                SyntaxKind.LogicalNotExpression,
                                node.Left
                            );
                        }
                    }
                }

                // Replace the right side with a boolean expression
                if (rightType.ConvertedType?.SpecialType == SpecialType.System_Boolean
                    && node.Left is LiteralExpressionSyntax literalNode
                    && literalNode.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    var numericLiteral = (int) literalNode.Token.Value;
                    if (numericLiteral == 1)
                    {
                        Count++;
                        return node.Right;
                    }
                    else if (numericLiteral == 0)
                    {
                        Count++;
                        return PrefixUnaryExpression(
                            SyntaxKind.LogicalNotExpression,
                            node.Right
                        );
                    }
                }
            }
            else if (node.IsKind(SyntaxKind.LogicalOrExpression) || node.IsKind(SyntaxKind.LogicalAndExpression))
            {
                bool convertLeft = HasIntOrEnumType(node.Left);
                bool leftRefType = HasReferenceType(node.Left);
                bool convertRight = HasIntOrEnumType(node.Right);
                bool rightRefType = HasReferenceType(node.Right);

                var newBase = base.VisitBinaryExpression(node);
                if (newBase is BinaryExpressionSyntax newBaseBse)
                {
                    if (convertLeft)
                    {
                        newBaseBse = newBaseBse.WithLeft(ConvertIntToBool(newBaseBse.Left));                        
                    }
                    else if (leftRefType)
                    {
                        newBaseBse = newBaseBse.WithLeft(CompareNotNull(newBaseBse.Left));
                    }

                    if (convertRight)
                    {
                        newBaseBse = newBaseBse.WithRight(ConvertIntToBool(newBaseBse.Right));
                    }
                    else if (rightRefType)
                    {
                        newBaseBse = newBaseBse.WithRight(CompareNotNull(newBaseBse.Right));
                    }
                    return newBaseBse;
                }
                return newBase;
            }
            
            return base.VisitBinaryExpression(node);
        }

        private static ExpressionSyntax CompareNotNull(ExpressionSyntax e)
        {
            return BinaryExpression(SyntaxKind.NotEqualsExpression, e, LiteralExpression(SyntaxKind.NullLiteralExpression));
        }
    }
}
