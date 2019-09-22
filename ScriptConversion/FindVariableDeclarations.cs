using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Compiler.Ast;
using IronPython.Modules;

namespace ScriptConversion
{
    internal class FindVariableDeclarations : PythonWalker
    {
        private readonly ExpressionConverter _expressionConverter;

        public Dictionary<string, GuessedType> NewVariables { get; } = new Dictionary<string, GuessedType>();

        public FindVariableDeclarations(ExpressionConverter expressionConverter)
        {
            _expressionConverter = expressionConverter;
        }

        // A for statement will also define a variable temporarily
        public override bool Walk(ForStatement node)
        {
            if (node.Left is NameExpression nameExpression)
            {
                var expressionConverter = _expressionConverter.Clone();
                expressionConverter.AddVariables(NewVariables);

                var newVar = nameExpression.Name;
                var newVarType = expressionConverter.GetListElementType(expressionConverter.GetExpressionType(node.List));
                NewVariables[newVar] = newVarType;

                node.Body?.Walk(this);
                node.Else?.Walk(this);

                NewVariables.Remove(newVar);
                return false;
            }

            return base.Walk(node);
        }

        public override bool Walk(AssignmentStatement node)
        {
            foreach (var leftExpression in node.Left)
            {
                if (leftExpression is NameExpression nameExpression)
                {
                    var variableName = nameExpression.Name;
                    if (!_expressionConverter.IsVariableDefined(variableName))
                    {
                        var expressionConverter = _expressionConverter.Clone();
                        expressionConverter.AddVariables(NewVariables);

                        var rhsType = expressionConverter.GetExpressionType(node.Right);
                        if (!NewVariables.TryGetValue(variableName, out var existingType)
                            || existingType == GuessedType.Unknown)
                        {
                            // Only overwrite the decl if the type is more precise
                            NewVariables[variableName] = rhsType;
                        }
                    }
                }
            }

            return true;
        }
    }
}