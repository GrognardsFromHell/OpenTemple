using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTemple.Core;

namespace DocGenerator
{
    public class SourceVisitor : CSharpSyntaxWalker
    {
        private readonly string _baseDir;

        public SourceVisitor(string baseDir)
        {
            _baseDir = baseDir;
        }

        public SemanticModel SemanticModel { get; set; }

        public Dictionary<string, ConditionSpecSource> ConditionSpecs { get; } =
            new Dictionary<string, ConditionSpecSource>();

        private static FieldDeclarationSyntax FindParentField(SyntaxNode node)
        {
            if (node is FieldDeclarationSyntax fds)
            {
                return fds;
            }

            if (node.Parent == null)
            {
                return null;
            }

            return FindParentField(node.Parent);
        }

        /// <summary>
        /// Searches for invocations of the Create method on ConditionSpec.
        /// </summary>
        /// <param name="node"></param>
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax maes &&
                maes.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var exprType = SemanticModel.GetTypeInfo(maes.Expression);
                if (exprType.Type.Name == "ConditionSpec")
                {
                    if (maes.Name is IdentifierNameSyntax methodName && methodName.Identifier.ValueText == "Create")
                    {
                        var argVisitor = new MethodArgumentVisitor(SemanticModel);
                        argVisitor.Visit(node.ArgumentList);

                        // This is a call such as ConditionSpec.Create!
                        // Seek upwards until we find the field declaration
                        var fieldDecl = FindParentField(node);

                        var initializerValue = fieldDecl.Declaration.Variables[0].Initializer.Value.ToFullString();
                        initializerValue = StripLeadingWhitespace(initializerValue);

                        var templeDllLocation = FindTempleDllLocation(fieldDecl);

                        var lineSpan = fieldDecl.GetLocation().GetLineSpan();
                        var location = Path.GetRelativePath(_baseDir, lineSpan.Path);
                        location += ":" + lineSpan.StartLinePosition.Line;

                        ConditionSpecs[argVisitor.ConditionName] = new ConditionSpecSource
                        {
                            Location = location,
                            Definition = initializerValue,
                            TempleDllLocation = templeDllLocation
                        };
                    }
                }
            }

            base.VisitInvocationExpression(node);
        }

        private static string StripLeadingWhitespace(string initializerValue)
        {
            var lines = initializerValue.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(' '))
                {
                    lines[i] = "  " + lines[i].TrimStart();
                }
            }

            return string.Join('\n', lines);
        }

        private string FindTempleDllLocation(FieldDeclarationSyntax fieldDecl)
        {
            foreach (var attrList in fieldDecl.AttributeLists)
            {
                foreach (var attrDecl in attrList.Attributes)
                {
                    var attrType = SemanticModel.GetTypeInfo(attrDecl);
                    if (attrType.Type.Name == typeof(TempleDllLocationAttribute).Name)
                    {
                        return attrDecl.ArgumentList.Arguments[0].Expression.ToString();
                    }
                }
            }

            return null;
        }
    }

    public class ConditionSpecSource
    {
        public string Location { get; set; }

        public string Definition { get; set; }

        public string TempleDllLocation { get; set; }
    }
}