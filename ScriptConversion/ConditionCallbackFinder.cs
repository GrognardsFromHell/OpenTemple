using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;
using OpenTemple.Core.Systems.D20;

namespace ScriptConversion;

/// <summary>
/// Walks the Python AST and searches for top-level function calls of the form:
/// classSpecObj.AddHook(ET_OnToHitBonusBase, EK_NONE, OnGetToHitBonusBase, ())
/// In order to figure out, which top-level functions serve as condition callbacks to rewrite their arguments.
/// </summary>
internal class ConditionCallbackFinder : PythonWalker
{
    private readonly ISet<string> _conditionVars = new HashSet<string>();

    public Dictionary<string, ConditionCallback> CallbackFunctions { get; } =
        new Dictionary<string, ConditionCallback>();

    public override bool Walk(FunctionDefinition node)
    {
        // Do not descend into function definitions, we only want to process top-level statements
        return false;
    }

    // Track which variables are PythonModifier objects so we'll be able to know
    // when methods are called on them
    public override void PostWalk(AssignmentStatement node)
    {
        if (node.Right is CallExpression callExpression
            && callExpression.Target is NameExpression nameExpression
            && nameExpression.Name == "PythonModifier")
        {
            foreach (var expression in node.Left)
            {
                if (expression is NameExpression variableNameExpression)
                {
                    _conditionVars.Add(variableNameExpression.Name);
                }
            }
        }
    }

    public override bool Walk(CallExpression node)
    {
        if (node.Target is MemberExpression memberExpression
            && memberExpression.Target is NameExpression callTargetNameExpression
            && _conditionVars.Contains(callTargetNameExpression.Name))
        {
            var methodName = memberExpression.Name;
            var args = node.Args;

            if (methodName == "AddHook"
                && args[0].Expression is NameExpression dispTypeNameExpr
                && args[2].Expression is NameExpression callbackFunctionNameExpr)
            {
                var dispatcherTypeStr = PythonConstants.Constants[dispTypeNameExpr.Name];
                var dispatcherType =
                    Enum.Parse<DispatcherType>(dispatcherTypeStr.Substring("DispatcherType.".Length));

                var functionName = callbackFunctionNameExpr.Name;
                if (CallbackFunctions.TryGetValue(functionName, out var callbackFunc))
                {
                    callbackFunc.UsedForDispatchers.Add(dispatcherType);
                }
                else
                {
                    CallbackFunctions[functionName] = new ConditionCallback
                    {
                        Name = functionName,
                        UsedForDispatchers = {dispatcherType}
                    };
                }
            }
        }

        return false;
    }
}

internal class ConditionCallback
{
    public string Name { get; set; }

    public ISet<DispatcherType> UsedForDispatchers { get; } = new HashSet<DispatcherType>();
}