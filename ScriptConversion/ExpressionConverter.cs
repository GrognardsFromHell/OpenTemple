using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using IronPython.Compiler.Ast;
using IronPython.Modules;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using BinaryExpression = IronPython.Compiler.Ast.BinaryExpression;
using ConditionalExpression = IronPython.Compiler.Ast.ConditionalExpression;
using ConstantExpression = IronPython.Compiler.Ast.ConstantExpression;
using Expression = IronPython.Compiler.Ast.Expression;
using IndexExpression = IronPython.Compiler.Ast.IndexExpression;
using LambdaExpression = IronPython.Compiler.Ast.LambdaExpression;
using MemberExpression = IronPython.Compiler.Ast.MemberExpression;
using Node = IronPython.Compiler.Ast.Node;
using PythonOperator = IronPython.Compiler.PythonOperator;
using UnaryExpression = IronPython.Compiler.Ast.UnaryExpression;

namespace ScriptConversion
{
    internal class ExpressionConverter : PythonWalker
    {
        private readonly Typings _typings;

        public readonly StringBuilder Result = new StringBuilder();

        private readonly Dictionary<string, GuessedType> _variables = new Dictionary<string, GuessedType>();

        private readonly PythonScript _currentScript;

        private readonly Dictionary<string, PythonScript> _modules;

        public GuessedType? ReturnType { get; set; }

        public FunctionDefinition CurrentFunction { get; set; }

        public bool QualifyCurrentScriptName { get; set; }

        private bool _processComments = false;

        private int _endOfLastNode;

        public ExpressionConverter(PythonScript currentScript,
            Typings typings,
            Dictionary<string, PythonScript> modules)
        {
            _currentScript = currentScript;
            _typings = typings;
            _modules = modules;
        }

        public void AddVariables(Dictionary<string, GuessedType> variables)
        {
            foreach (var kvp in variables)
            {
                _variables[kvp.Key] = kvp.Value;
            }
        }

        // (f & 123) -> (f & 123) != 0
        private Expression FixBitfieldComparison(Expression expression)
        {
            if (!(UnwrapParenthesis(expression) is BinaryExpression binaryExpression)
                || binaryExpression.Operator != PythonOperator.BitwiseAnd)
            {
                return expression;
            }

            return new BinaryExpression(
                PythonOperator.NotEqual,
                new ParenthesisExpression(expression),
                new ConstantExpression(0)
            );
        }

        public override bool Walk(AndExpression node)
        {
            var leftNode = FixBitfieldComparison(node.Left);
            var rightNode = FixBitfieldComparison(node.Right);

            leftNode.Walk(this);
            Result.Append(" && ");
            rightNode.Walk(this);
            return false;
        }

        public override bool Walk(BackQuoteExpression node)
        {
            if (node.Expression is NameExpression nameExpression)
            {
                Result.Append(nameExpression.Name).Append(".ToString()");
            }
            else
            {
                Result.Append("(");
                node.Expression.Walk(this);
                Result.Append(").ToString()");
            }

            _lastType = GuessedType.String;
            return false;
        }

        private static Expression TranslateD20CAFConstant(Expression expression)
        {
            if (!(expression is ConstantExpression constantExpression)
                || !(constantExpression.Value is int intValue))
            {
                return expression;
            }

            switch (intValue)
            {
                case 0:
                    return new NameExpression("D20CAF_NONE");
                case 1:
                    return new NameExpression("D20CAF_HIT");
                case 2:
                    return new NameExpression("D20CAF_CRITICAL");
                default:
                    return expression;
            }
        }

        public override bool Walk(BinaryExpression node)
        {
            var left = node.Left;
            var right = node.Right;
            var leftType = GetExpressionType(node.Left);
            var rightType = GetExpressionType(node.Right);
            var op = node.Operator;

            if (op == PythonOperator.Power)
            {
                // Special case handling for bitfield magic
                if (IsInteger(left, 2))
                {
                    if (right is ConstantExpression constantExpression && constantExpression.Value is int intValue)
                    {
                        Result.Append($"0x{1 << intValue:x}");
                    }
                    else
                    {
                        Result.Append("(1 << ");
                        right.Walk(this);
                        Result.Append(")");
                    }

                    _lastType = GuessedType.Integer;
                    return false;
                }

                Result.Append("Math.Pow(");
                left.Walk(this);
                Result.Append(", ");
                right.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Float;
                return false;
            }

            // Convert bitwise operations on alignment to extension methods
            if (op == PythonOperator.BitwiseAnd
                && leftType == GuessedType.Alignment
                && right is NameExpression alignmentNameExpr)
            {
                bool AppendAlignmentCheck(string name)
                {
                    left.Walk(this);
                    Result.Append(".");
                    Result.Append(name);
                    Result.Append("()");
                    _lastType = GuessedType.Bool;
                    return false;
                }

                switch (alignmentNameExpr.Name)
                {
                    case "ALIGNMENT_LAWFUL":
                    case "ALIGNMENT_LAWFUL_NEUTRAL":
                        return AppendAlignmentCheck(nameof(AlignmentExtensions.IsLawful));

                    case "ALIGNMENT_CHAOTIC":
                    case "ALIGNMENT_CHAOTIC_NEUTRAL":
                        return AppendAlignmentCheck(nameof(AlignmentExtensions.IsChaotic));

                    case "ALIGNMENT_GOOD":
                    case "ALIGNMENT_NEUTRAL_GOOD":
                        return AppendAlignmentCheck(nameof(AlignmentExtensions.IsGood));

                    case "ALIGNMENT_EVIL":
                    case "ALIGNMENT_NEUTRAL_EVIL":
                        return AppendAlignmentCheck(nameof(AlignmentExtensions.IsEvil));

                    default:
                        Console.WriteLine("Bitwise operation with nonsensical alignment: " + alignmentNameExpr.Name);
                        break;
                }
            }
            // Convert random encounter query flags to "type" checks
            else if (op == PythonOperator.BitwiseAnd && leftType == GuessedType.RandomEncounterType)
            {
                op = PythonOperator.Equal;
            }

            // Heuristic check: if left or right of node are of type boolean, and the other node
            // is an integer constant of 1 or 0, convert to a unary expression
            if (op == PythonOperator.Equal || op == PythonOperator.NotEqual ||
                op == PythonOperator.GreaterThan)
            {
                // Quick fix for this in python: a & 1 == 1, which is valid there, but not in C#
                if (left is BinaryExpression leftBinExpr && leftBinExpr.Operator == PythonOperator.BitwiseAnd)
                {
                    left = new ParenthesisExpression(left);
                }

                // Yeah, sometimes they actually compare using x > 0 :|
                var negate = op == PythonOperator.NotEqual || op == PythonOperator.GreaterThan;
                var falseInt = negate ? 1 : 0;
                var trueInt = negate ? 0 : 1;

                if (leftType == GuessedType.Bool)
                {
                    if (!negate && IsInteger(node.Right, falseInt))
                    {
                        Result.Append("!");
                        node.Left.Walk(this);
                        return false;
                    }
                    else if (IsInteger(node.Right, trueInt))
                    {
                        node.Left.Walk(this);
                        return false;
                    }
                }
                else if (rightType == GuessedType.Bool)
                {
                    if (IsInteger(node.Left, falseInt))
                    {
                        Result.Append("!");
                        node.Right.Walk(this);
                        return false;
                    }
                    else if (IsInteger(node.Left, trueInt))
                    {
                        node.Right.Walk(this);
                        return false;
                    }
                }
                else if (leftType == GuessedType.D20CAF)
                {
                    right = TranslateD20CAFConstant(right);
                }
                else if (rightType == GuessedType.D20CAF)
                {
                    left = TranslateD20CAFConstant(left);
                }
            }

            // Special handling for the IN operator.
            // LHS must be the element
            // RHS must be some sort of list...
            if (op == PythonOperator.In || op == PythonOperator.NotIn)
            {
                if (op == PythonOperator.NotIn)
                {
                    Result.Append("!");
                }

                Result.Append('(');
                right.Walk(this);
                Result.Append(").Contains(");
                left.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Bool;
                return false;
            }

            left.Walk(this);
            Result.Append(' ');
            Result.Append(GetOperatorSymbol(op));
            Result.Append(' ');
            right.Walk(this);

            if (op == PythonOperator.Equal
                || op == PythonOperator.NotEqual
                || op == PythonOperator.Not)
            {
                _lastType = GuessedType.Bool;
            }

            return false;
        }

        private string GetOperatorSymbol(PythonOperator nodeOperator)
        {
            switch (nodeOperator)
            {
                case PythonOperator.Add:
                    return "+";
                case PythonOperator.Subtract:
                    return "-";
                case PythonOperator.Multiply:
                    return "*";
                case PythonOperator.Divide:
                    return "/";
                case PythonOperator.TrueDivide:
                    return "/";
                case PythonOperator.Mod:
                    return "%";
                case PythonOperator.LessThan:
                    return "<";
                case PythonOperator.LessThanOrEqual:
                    return "<=";
                case PythonOperator.GreaterThan:
                    return ">";
                case PythonOperator.GreaterThanOrEqual:
                    return ">=";
                case PythonOperator.Equal:
                    return "==";
                case PythonOperator.NotEqual:
                    return "!=";
                case PythonOperator.Pos:
                    return "+";
                case PythonOperator.Negate:
                    return "-";
                case PythonOperator.Not:
                    return "!";
                case PythonOperator.BitwiseAnd:
                    return "&"; // TODO: These are mostly fishy in a python script TBH
                case PythonOperator.BitwiseOr:
                    return "|"; // TODO: These are mostly fishy in a python script TBH
                case PythonOperator.RightShift:
                    return ">>"; // TODO: These are mostly fishy in a python script TBH
                case PythonOperator.LeftShift:
                    return "<<"; // TODO: These are mostly fishy in a python script TBH
                case PythonOperator.Invert:
                    return "~";
                case PythonOperator.Xor:
                    return "^";
                case PythonOperator.Is:
                    return " is ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeOperator), nodeOperator,
                        "Unsupported node operator.");
            }
        }

        public override bool Walk(CallExpression node)
        {
            // Handle translation of well known methods that call into the API, and which are not local functions
            if (node.Target is MemberExpression memberExpression)
            {
                var target = memberExpression.Target;
                var methodName = memberExpression.Name;
                var args = node.Args.Select(e => e.Expression).ToArray();
                var expressionType = GetExpressionType(target);
                if (IsGameGlobal(target))
                {
                    // Since game is a singleton, no need to pass target
                    ConvertGameMethod(methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.Object)
                {
                    ConvertObjectMethod(target, methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.Dice)
                {
                    ConvertDiceMethod(target, methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.Spell)
                {
                    ConvertSpellMethod(target, methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.SpellTargets)
                {
                    ConvertSpellTargetsMethod(target, methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.TrapSprungEvent)
                {
                    ConvertTrapSprungMethod(target, methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.EncounterQueue)
                {
                    ConvertEncounterQueueMethod(target, methodName, args, out _lastType);
                    return false;
                }
                else if (expressionType == GuessedType.ObjectList && methodName == "append")
                {
                    target.Walk(this);
                    Result.Append(".Add");
                    Result.Append("(");
                    PrintArguments(args);
                    Result.Append(")");
                    return false;
                }
                else if (expressionType == GuessedType.ConditionSpec)
                {
                    if (methodName == "AddHook")
                    {
                        // Skip EK_NONE
                        MapToMethod(target, "AddHandler", args.Where(t => NodeToString(t) != "EK_NONE").ToArray());
                    }
                    else
                    {
                        MapToMethod(target, methodName, args);
                    }

                    return false;
                }
                else if (expressionType == GuessedType.SpecialConditionArguments)
                {
                    // NOTE how this ignores the actuaul name of the arguments, since this is a special case
                    // we know that "evt" will be in the current scope
                    Result.Append("evt.");
                    if (methodName == "get_arg")
                    {
                        // We do not force a conversion to int here because it might be a variable
                        var argIndexStr = NodeToString(args[0]);
                        if (args.Length != 1)
                        {
                            throw new InvalidOperationException("Expected argument array to have one entry.");
                        }

                        switch (argIndexStr)
                        {
                            case "0":
                                Result.Append("GetConditionArg1()");
                                break;
                            case "1":
                                Result.Append("GetConditionArg2()");
                                break;
                            case "2":
                                Result.Append("GetConditionArg3()");
                                break;
                            case "3":
                                Result.Append("GetConditionArg4()");
                                break;
                            default:
                                Result.Append("GetConditionArg(");
                                Result.Append(argIndexStr);
                                Result.Append(")");
                                break;
                        }

                        _lastType = GuessedType.Integer;

                        return false;
                    }
                    else if (methodName == "set_arg")
                    {
                        // We do not force a conversion to int here because it might be a variable
                        var argIndexStr = NodeToString(args[0]);

                        switch (argIndexStr)
                        {
                            case "0":
                                Result.Append("SetConditionArg1(");
                                PrintArguments(args.Skip(1).ToArray());
                                Result.Append(")");
                                break;
                            case "1":
                                Result.Append("SetConditionArg2(");
                                PrintArguments(args.Skip(1).ToArray());
                                Result.Append(")");
                                break;
                            case "2":
                                Result.Append("SetConditionArg3(");
                                PrintArguments(args.Skip(1).ToArray());
                                Result.Append(")");
                                break;
                            case "3":
                                Result.Append("SetConditionArg4(");
                                PrintArguments(args.Skip(1).ToArray());
                                Result.Append(")");
                                break;
                            default:
                                Result.Append("SetConditionArg(");
                                PrintArguments(args);
                                Result.Append(")");
                                break;
                        }

                        return false;
                    }
                    else
                    {
                        // We don't know how to handle this method
                        Result.Append(methodName);
                        Result.Append("(");
                        PrintArguments(args);
                        Result.Append(")");
                    }

                    return false;
                }
                else if (expressionType == GuessedType.AttackPacket)
                {
                    TranslateAttackPacketProperty(memberExpression);
                    Result.Append("(");
                    PrintArguments(args);
                    Result.Append(")");
                    return false;
                }
                else if (expressionType == GuessedType.BonusList)
                {
                    TranslateBonusListProperty(memberExpression);
                    Result.Append("(");
                    PrintArguments(args);
                    Result.Append(")");
                    return false;
                }
            }
            else if (node.Target is NameExpression nameExpression)
            {
                // Call to named function, check if it's one we might want to translate
                var args = node.Args.Select(e => e.Expression).ToArray();
                var funcName = nameExpression.Name;
                // anyone( triggerer.group_list(), "has_follower", 8014 )
                if (funcName == "anyone" && args.Length == 3)
                {
                    Trace.Assert(!_variables.ContainsKey("o"));

                    var methodName = (string) ((ConstantExpression) args[1]).Value;

                    // We assume the first argument to be an IEnumerable<GameObjectBody>, so that we can use LINQ here
                    args[0].Walk(this);
                    Result.Append(".Any(o => ");
                    // Synthesize a call expression to a temporary variable
                    _variables["o"] = GuessedType.Object;
                    new CallExpression(
                        new MemberExpression(new NameExpression("o"), methodName),
                        new[] {node.Args[2]}
                    ).Walk(this);
                    // Cheap and dirty, but fixes alot of these calls
                    if (_lastType == GuessedType.Object)
                    {
                        Result.Append(" != null");
                    }

                    _variables.Remove("o");
                    Result.Append(")");
                    _lastType = GuessedType.Bool;
                    return false;
                }
                else if (funcName == "dice_new" && node.Args.Count == 1)
                {
                    return TranslateDiceNew(node);
                }
                else if (funcName == "len" && node.Args.Count == 1)
                {
                    node.Args[0].Walk(this);
                    Result.Append(".Count");
                    _lastType = GuessedType.Integer;
                    return false;
                }
                else if (funcName == "dice_new" && node.Args.Count == 3)
                {
                    Result.Append("new Dice(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Dice;
                    return false;
                }
                else if (funcName == "min")
                {
                    Result.Append("Math.Min(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Bool;
                    return false;
                }
                else if (funcName == "max")
                {
                    Result.Append("Math.Max(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Bool;
                    return false;
                }
                else if (funcName == "abs")
                {
                    Result.Append("Math.Abs(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Bool;
                    return false;
                }
                else if (funcName == "cos")
                {
                    Result.Append("MathF.Cos(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Float;
                    return false;
                }
                else if (funcName == "sin")
                {
                    Result.Append("MathF.Sin(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Float;
                    return false;
                }
                else if (funcName == "abs")
                {
                    Result.Append("Math.Abs(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Bool;
                    return false;
                }
                else if (funcName == "sqrt")
                {
                    Result.Append("MathF.Sqrt(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Bool;
                    return false;
                }
                else if (funcName == "pow")
                {
                    // This function is primarily used to build bitmasks
                    if (IsInteger(args[0], 2))
                    {
                        Result.Append("(1 << ");
                        args[1].Walk(this);
                        Result.Append(")");
                        _lastType = GuessedType.Integer;
                    }
                    else
                    {
                        Result.Append("Math.Pow(");
                        PrintArguments(args);
                        Result.Append(")");
                        _lastType = GuessedType.Float;
                    }

                    return false;
                }
                else if (funcName == "type")
                {
                    Result.Append("typeof(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Unknown;
                    return false;
                }
                // Special casing for Co8 random encounter
                else if (funcName == "RE_entry")
                {
                    Result.Append("new RE_entry(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Unknown;
                    return false;
                }
                else if ((funcName == "str" || funcName == "str") && args.Length == 1)
                {
                    ParenthesisForCall(args[0]).Walk(this);
                    Result.Append(".ToString()");
                    _lastType = GuessedType.String;
                    return false;
                }
                else if (funcName == "int")
                {
                    // This is mostly used to floor something
                    Result.Append("(int)(");
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Integer;
                    return false;
                }
                else if (funcName == "PythonModifier")
                {
                    // This is Temple+ Condition Code
                    Result.Append("ConditionSpec.Create(");
                    PrintArguments(args);
                    Result.Append(").Build()");
                    _lastType = GuessedType.ConditionSpec;
                    return false;
                }
            }

            var qualifiedName = NodeToString(node.Target);
            GuessedType returnType;

            // Replace this with the locXY constructor
            if (qualifiedName == "location_from_axis" || qualifiedName == "utilities.location_from_axis")
            {
                Result.Append("new locXY");
                returnType = GuessedType.Location;
            }

            else if (qualifiedName == "list" && node.Args.Count == 1)
            {
                // Unwrap list() calls because they're only used in contexts where the RHS is practically a list
                node.Args[0].Expression.Walk(this);
                return false;
            }

            else if ((qualifiedName == "location_to_axis" || qualifiedName == "utilities.location_to_axis")
                     && node.Args.Count == 1)
            {
                // Completely unwrap the call, because it's usually used in a destructuring expression,
                // which our locXY now supports
                node.Args[0].Expression.Walk(this);
                return false;
            }

            // There may be other modules that were imported who export this function
            else if (FindExportedFunction(qualifiedName, out var exportingModule, out var exportedFunction))
            {
                if (exportingModule != _currentScript && !QualifyCurrentScriptName)
                {
                    Result.Append(exportingModule.ClassName);
                    Result.Append('.');
                }

                Result.Append(exportedFunction.CSharpName);
                returnType = exportedFunction.ReturnType;
            }
            else
            {
                // Don't warn if the function is defined in the same class
                var definedFunctionWalker = new DefinedFunctionWalker();
                node.Parent.Walk(definedFunctionWalker);
                if (!definedFunctionWalker.DefinedFunctions.Contains(qualifiedName))
                {
                    Console.WriteLine("Call to unknown function detected: " + NodeToString(node));
                }

                node.Target.Walk(this);
                returnType = GuessedType.Unknown;
            }

            Result.Append('(');
            for (var index = 0; index < node.Args.Count; index++)
            {
                if (index > 0)
                {
                    Result.Append(", ");
                }

                var nodeArg = node.Args[index];
                if (nodeArg.Name != null)
                {
                    Result.Append(nodeArg.Name);
                    Result.Append(':');
                }

                nodeArg.Expression.Walk(this);
            }

            Result.Append(')');
            _lastType = returnType;
            return false;
        }

        // relatively simplistic check for stuff like a + b to make it (a + b).SomeMethod()
        private Expression ParenthesisForCall(Expression expression)
        {
            if (expression is BinaryExpression || expression is UnaryExpression)
            {
                return new ParenthesisExpression(expression);
            }

            return expression;
        }

        private void ConvertTrapSprungMethod(Expression target, string methodName, Expression[] args,
            out GuessedType lastType)
        {
            ConvertMethodUsingExtensionClass(typeof(ScriptTrapExtensions),
                target,
                methodName,
                args,
                out lastType);
        }

        private void ConvertEncounterQueueMethod(Expression target, string methodName, Expression[] args,
            out GuessedType lastType)
        {
            switch (methodName)
            {
                case "append":
                    Result.Append("QueueRandomEncounter(");
                    PrintArguments(args);
                    Result.Append(")");
                    lastType = GuessedType.Unknown;
                    return;
                default:
                    throw new NotSupportedException("Unknown method called on encounter queue: " + methodName);
            }
        }

        private void ConvertDiceMethod(Expression target, string methodName, Expression[] args,
            out GuessedType lastType)
        {
            if (args.Length != 0)
            {
                throw new NotSupportedException("None of the Dice methods take arguments");
            }

            target.Walk(this);
            Result.Append(".");
            switch (methodName)
            {
                case "clone":
                    Result.Append(nameof(Dice.Copy));
                    Result.Append("()");
                    lastType = GuessedType.Dice;
                    return;
                case "roll":
                    Result.Append(nameof(Dice.Roll));
                    Result.Append("()");
                    lastType = GuessedType.Integer;
                    return;
                default:
                    throw new NotSupportedException($"Unknown dice method: '{methodName}'");
            }
        }

        private bool TranslateDiceNew(CallExpression node)
        {
            // We can "pre-parse" constants and reuse constant dice
            var arg = node.Args[0].Expression;
            if (TryGetStringConstant(arg, out var diceText))
            {
                if (!Dice.TryParse(diceText, out var dice))
                {
                    throw new NotSupportedException("Invalid dice spec found: '" + diceText + "'");
                }

                // Transform into a constant die
                if (dice.Sides == 0 || dice.Count == 0)
                {
                    Result.Append("Dice.Constant(");
                    Result.Append(dice.Modifier);
                    Result.Append(")");
                }
                else if (dice.Count == 1 && dice.Modifier == 0)
                {
                    switch (dice.Sides)
                    {
                        case 2:
                        case 3:
                        case 4:
                        case 6:
                        case 8:
                        case 10:
                        case 12:
                        case 20:
                        case 100:
                            Result.Append("Dice.D").Append(dice.Sides);
                            break;
                        default:
                            Result.Append("Dice.Parse(\"").Append(diceText).Append("\")");
                            break;
                    }
                }
                else
                {
                    Result.Append("Dice.Parse(\"").Append(diceText).Append("\")");
                }
            }

            _lastType = GuessedType.Dice;
            return false;
        }

        private bool FindExportedFunction(string qualifiedName,
            out PythonScript exportingModule,
            out ExportedFunction exportedFunction)
        {
            exportingModule = null;
            exportedFunction = null;

            if (qualifiedName.Contains("."))
            {
                var parts = qualifiedName.Split(".");
                if (parts.Length != 2)
                {
                    return false;
                }

                if (!_modules.TryGetValue(parts[0], out var module))
                {
                    return false;
                }

                if (!module.ExportedFunctions.TryGetValue(parts[1], out exportedFunction))
                {
                    return false;
                }

                // Check typings and inherit fixed return type from CSV file
                if (_typings.TryGetSignature(module.ClassName, parts[1], out var typedReturnType, out _))
                {
                    exportedFunction.ReturnType = typedReturnType;
                }

                exportingModule = module;
                return true;
            }
            else if (_modules.TryGetValue(_currentScript.ModuleName, out var existingModule)
                     && existingModule.ExportedFunctions.ContainsKey(qualifiedName))
            {
                // It might be coming from this module
                exportingModule = _currentScript;
                exportedFunction = existingModule.ExportedFunctions[qualifiedName];
                return true;
            }

            // This may be typed (either locally or *)
            if (_typings.TryGetSignature(_currentScript.ClassName, qualifiedName, out var actualReturnType, out _))
            {
                exportingModule = _currentScript;
                exportedFunction = new ExportedFunction
                {
                    CSharpName = qualifiedName,
                    PythonName = qualifiedName,
                    ReturnType = actualReturnType
                };
                return true;
            }

            var candidateModules = _modules.Where(kvp => kvp.Value.ExportedFunctions.ContainsKey(qualifiedName))
                .Where(kvp => _currentScript.ImportedModules.Contains(kvp.Key) || ImplicitModules.Contains(kvp.Key))
                .ToList();

            // If there's a SINGLE candidate that's a module, prefer that
            if (candidateModules.Count > 1)
            {
                var scriptCandidates = candidateModules.Where(kvp => kvp.Value.Type == ScriptType.Module)
                    .ToList();
                if (scriptCandidates.Count == 1)
                {
                    candidateModules = scriptCandidates;
                }
            }

            if (candidateModules.Count > 1)
            {
                Console.WriteLine("Ambiguous function: " + qualifiedName + " found in "
                                  + string.Join(", ", candidateModules.Select(m => m.Key)));
            }
            else if (candidateModules.Count == 1)
            {
                exportingModule = candidateModules[0].Value;
                exportedFunction = exportingModule.ExportedFunctions[qualifiedName];
                return true;
            }

            exportedFunction = null;
            return false;
        }

        private void PrintArguments(params Expression[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    Result.Append(", ");
                }

                args[i].Walk(this);
            }
        }

        internal GuessedType GetExpressionType(Expression expression)
        {
            var converter = Clone();
            expression.Walk(converter);
            return converter._lastType;
        }

        private void ConvertGameMethod(string methodName, Expression[] args, out GuessedType resultType)
        {
            resultType = GuessedType.Unknown;

            void AssertArgCount(int count)
            {
                if (args.Length != count)
                {
                    throw new ArgumentException($"Invalid number of arguments passed to {methodName}: {args}");
                }
            }

            void MapDirect(string newName)
            {
                Result.Append(newName);
                Result.Append("(");
                PrintArguments(args);
                Result.Append(")");
            }

            switch (methodName)
            {
                case "party_size":
                    Result.Append("GameSystems.Party.PartySize");
                    resultType = GuessedType.Integer;
                    break;

                case "sound_local_obj":
                    MapDirect("GameSystems.SoundGame.PositionalSound");
                    break;

                case "particles":
                    AssertArgCount(2);
                    var arg1Type = GetExpressionType(args[1]);
                    if (arg1Type == GuessedType.Object)
                    {
                        MapDirect("AttachParticles");
                    }
                    else
                    {
                        MapDirect("SpawnParticles");
                    }

                    break;

                // In the instances I've seen, particles_end is always called with the same argument structure
                // which is the particle system id taken from the projectile object
                case "particles_end":
                    if (args.Length != 1)
                    {
                        throw new NotSupportedException();
                    }

                    // Check that it's an expression of the following kind:
                    // game.particles_end( projectile.obj_get_int( obj_f_projectile_part_sys_id ) )
                    if (!(args[0] is CallExpression callExpression)
                        || !(callExpression.Target is MemberExpression memberExpression)
                        || memberExpression.Name != "obj_get_int"
                        || callExpression.Args.Count != 1
                        || !(callExpression.Args[0].Expression is NameExpression fieldNameExpression)
                        || fieldNameExpression.Name != "obj_f_projectile_part_sys_id")
                    {
                        throw new NotSupportedException();
                    }

                    Result.Append("EndProjectileParticles(");
                    memberExpression.Target.Walk(this);
                    Result.Append(")");
                    break;

                case "ui_show_worldmap":
                    MapDirect("UiSystems.WorldMap.Show");
                    break;

                case "is_daytime":
                    Result.Append("GameSystems.TimeEvent.IsDaytime");
                    resultType = GuessedType.Bool;
                    break;

                case "get_stat_mod":
                    // TODO: Should use the appropriate modifier stat directly
                    MapDirect("FIXMEget_stat_mod");
                    break;

                case "particles_kill":
                    MapDirect("GameSystems.ParticleSys.Remove");
                    break;

                case "pfx_lightning_bolt":
                    MapDirect("GameSystems.Vfx.LightningBolt");
                    break;

                case "pfx_call_lightning":
                    MapDirect("GameSystems.Vfx.CallLightning");
                    break;

                case "pfx_chain_lightning":
                    MapDirect("GameSystems.Vfx.ChainLightning");
                    break;

                case "shake":
                    MapDirect("GameSystems.Scroll.ShakeScreen");
                    break;

                case "timevent_add":
                case "timeevent_add":
                {
                    // We're reordering arguments here to support a varargs approach to passing arguments
                    if (args.Length != 3 && args.Length != 4)
                    {
                        throw new NotSupportedException("Time event was called with more than 3 args (or less!): "
                                                        + string.Join(", ", args.Select(NodeToString)));
                    }

                    var callbackExpr = args[0];
                    var eventArgsExpr = args[1];
                    var elapseInExpr = args[2];
                    string callbackName;
                    if (callbackExpr is NameExpression nameExpression)
                    {
                        callbackName = nameExpression.Name;
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported timeevent callback:" + NodeToString(callbackExpr));
                    }

                    IList<Expression> callbackArgs;
                    if (eventArgsExpr is TupleExpression tupleExpression)
                    {
                        callbackArgs = tupleExpression.Items;
                    }
                    else if (eventArgsExpr is ParenthesisExpression parenthesisExpression)
                    {
                        // Bug in some vanilla scripts, where (arg) is used, which is equivalent to just arg in Python
                        callbackArgs = new[] {parenthesisExpression.Expression};
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    // callbackName may refer to a function from utilities, so we'll check for exported functions.
                    // But before we do that, we will actually check whether the current module defines such a function
                    if (!IsFunctionDefinedLocally(callbackExpr, callbackName))
                    {
                        if (FindExportedFunction(callbackName, out var exportingModule, out var exportedFunction))
                        {
                            callbackName = exportingModule.ClassName + "." + exportedFunction.CSharpName;
                        }
                        else
                        {
                            Console.WriteLine("Couldn't resolve callback for timeevent_add: " + callbackName);
                        }
                    }

                    Result.Append("StartTimer(");
                    elapseInExpr.Walk(this);
                    Result.Append(", ");
                    // The callback name can come from other modules as well, although we admittetly do not know that at this point...
                    Result.Append("() => " + callbackName + "(");
                    for (var index = 0; index < callbackArgs.Count; index++)
                    {
                        var item = callbackArgs[index];
                        if (index > 0)
                        {
                            Result.Append(", ");
                        }

                        item.Walk(this);
                    }

                    Result.Append(")");

                    if (args.Length == 4)
                    {
                        Result.Append(", ");
                        CoerceToBoolean(args[3]).Walk(this);
                    }

                    Result.Append(")");

                    break;
                }

                case "obj_create":
                    MapDirect("GameSystems.MapObject.CreateObject");
                    resultType = GuessedType.Object;
                    break;

                case "obj_list_vicinity":
                    MapDirect("ObjList.ListVicinity");
                    resultType = GuessedType.ObjectList;
                    break;

                case "obj_list_cone":
                    MapDirect("ObjList.ListCone");
                    resultType = GuessedType.ObjectList;
                    break;

                case "fade_and_teleport":
                    MapDirect("FadeAndTeleport");
                    break;

                case "fade":
                    MapDirect("Fade");
                    break;

                case "moviequeue_add":
                    MapDirect("GameSystems.Movies.MovieQueueAdd");
                    break;

                case "moviequeue_play":
                    MapDirect("GameSystems.Movies.MovieQueuePlay");
                    break;

                case "moviequeue_play_end_game":
                    MapDirect("GameSystems.Movies.MovieQueuePlayAndEndGame");
                    break;

                case "combat_is_active":
                    MapDirect("GameSystems.Combat.IsCombatActive");
                    break;

                case "random_range":
                    MapDirect("RandomRange");
                    resultType = GuessedType.Integer;
                    break;

                case "sleep_status_update":
                    MapDirect("GameSystems.RandomEncounter.UpdateSleepStatus");
                    break;

                case "brawl":
                    MapDirect("GameSystems.Combat.Brawl");
                    break;

                case "party_npc_size":
                    Result.Append("GameSystems.Party.NPCFollowersSize");
                    resultType = GuessedType.Integer;
                    break;

                case "party_pc_size":
                    Result.Append("GameSystems.Party.PlayerCharactersSize");
                    resultType = GuessedType.Integer;
                    break;

                case "sound":
                    MapDirect("Sound");
                    resultType = GuessedType.Unknown;
                    break;

                case "char_ui_hide":
                    MapDirect("UiSystems.CharSheet.Hide");
                    break;

                case "party_pool":
                    Result.Append("UiSystems.PartyPool.Show(true)");
                    break;

                case "tutorial_is_active":
                    if (args.Length != 0)
                    {
                        throw new NotSupportedException("Expected call to tutorial_is_active to have no arguments");
                    }

                    Result.Append("UiSystems.HelpManager.IsTutorialActive");
                    resultType = GuessedType.Bool;
                    break;

                case "tutorial_toggle":
                    MapDirect("UiSystems.HelpManager.ToggleTutorial");
                    break;

                case "tutorial_show_topic":
                    MapDirect("UiSystems.HelpManager.ShowTutorialTopic");
                    break;

                case "is_outdoor":
                    MapDirect("GameSystems.Map.IsCurrentMapOutdoors");
                    _lastType = GuessedType.Bool;
                    break;

                case "update_combat_ui":
                    MapDirect("UiSystems.Combat.Initiative.UpdateIfNeeded");
                    break;

                default:
                    Console.WriteLine("Unknown method called on game global: " + methodName);
                    Result.Append("/*FIXME*/" + methodName + "("); // TODO
                    PrintArguments(args);
                    Result.Append(")");
                    _lastType = GuessedType.Unknown;
                    break;
            }
        }

        private static bool IsFunctionDefinedLocally(Node context, string callbackName)
        {
            // Find the root of the AST
            var parent = context;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }

            var definedFunctionsWalker = new DefinedFunctionWalker();
            parent.Walk(definedFunctionsWalker);
            return definedFunctionsWalker.DefinedFunctions.Contains(callbackName);
        }

        private void ConvertAnimObjectMethod(string methodName, Expression[] args)
        {
            switch (methodName)
            {
                case "footstep":
                    Trace.Assert(args.Length == 0);
                    // Triggers a footstep sound appropriate for the animated object at the animated object's location
                    Result.Append("GameSystems.Critter.MakeFootstepSound(AnimatedObject)");
                    break;
                case "fade_to":
                    Result.Append("GameSystems.ObjFade.FadeTo(AnimatedObject, ");
                    PrintArguments(args);
                    Result.Append(")");
                    break;
                case "balor_death":
                    Result.Append("GameSystems.Critter.BalorDeath(AnimatedObject)");
                    break;
                default:
                    throw new NotSupportedException("Unknown method called on anim_obj: " + methodName);
            }
        }

        private void MapToMethod(Expression target, string newMethodName, Expression[] args)
        {
            target?.Walk(this);
            Result.Append('.');
            Result.Append(newMethodName);
            Result.Append('(');
            PrintArguments(args);
            Result.Append(')');
        }

        private void ConvertObjectMethod(Expression target, string methodName, Expression[] args,
            out GuessedType resultType)
        {
            // Special case for x.run_off(x.location - 3) and - 2-...
            if (methodName == "runoff" && args.Length == 1)
            {
                var callTarget = NodeToString(target);
                if (args[0] is BinaryExpression binaryExpression
                    && binaryExpression.Operator == PythonOperator.Subtract
                    && binaryExpression.Right is ConstantExpression rightConst
                    && rightConst.Value is int intValue
                    && (intValue == 3 || intValue == 2)
                    && binaryExpression.Left is MemberExpression leftMember
                    && leftMember.Name == "location"
                    && NodeToString(leftMember.Target) == callTarget)
                {
                    // let the RunOff function handle the run-off target automatically
                    args = Array.Empty<Expression>();
                }
            }
            // Conversion for certain GetStat calls into more convenient extensions that return the right type directly
            else if (methodName == "stat_level_get")
            {
                var statToken = NodeToString(args[0]);
                // Try figuring out which stat is being queried
                switch (statToken)
                {
                    case "stat_alignment":
                        MapToMethod(target, "GetAlignment", Array.Empty<Expression>());
                        resultType = GuessedType.Alignment;
                        return;
                    case "stat_gender":
                        MapToMethod(target, "GetGender", Array.Empty<Expression>());
                        resultType = GuessedType.Gender;
                        return;
                    case "stat_race":
                        MapToMethod(target, "GetRace", Array.Empty<Expression>());
                        resultType = GuessedType.Race;
                        return;
                }
            }
            // Conversion for certain GetInt32 calls into more convenient extension methods
            else if (methodName == "obj_get_int")
            {
                var statToken = NodeToString(args[0]);
                // Try figuring out which stat is being queried
                switch (statToken)
                {
                    case "obj_f_material":
                        MapToMethod(target, "GetMaterial", Array.Empty<Expression>());
                        resultType = GuessedType.Material;
                        return;
                }
            }
            // Setting the particle system of projectiles needs to be done differently since particle systems dont
            // have integer handles anymore
            else if (methodName == "obj_set_int"
                     && args.Length == 2
                     && args[0] is NameExpression fieldConstant
                     && fieldConstant.Name == "obj_f_projectile_part_sys_id")
            {
                Result.Append("SetProjectileParticles(");
                target.Walk(this);
                Result.Append(", ");
                args[1].Walk(this);
                Result.Append(")");
                resultType = GuessedType.Unknown; // Actually void...
                return;
            }

            ConvertMethodUsingExtensionClass(typeof(ScriptObjectExtensions), target, methodName, args,
                out resultType);
        }

        private void ConvertMethodUsingExtensionClass(Type extensionType, Expression target, string methodName,
            Expression[] args,
            out GuessedType resultType)
        {
            // Look for a method in ScriptObjectExtensions that has a matching attribute
            foreach (var member in extensionType.GetDeclaredMethods())
            {
                if (!member.IsStatic)
                {
                    continue;
                }

                foreach (var nameAttribute in member.GetCustomAttributes<PythonNameAttribute>())
                {
                    if (nameAttribute.Name == methodName)
                    {
                        // we're skipping the first param because it's the extension method "this" parameter
                        var targetParameters = member.GetParameters().Skip(1).ToArray();


                        // Found a matching method!
                        target?.Walk(this);
                        Result.Append('.');
                        Result.Append(member.Name);
                        Result.Append('(');
                        for (var i = 0; i < args.Length; i++)
                        {
                            if (i > 0)
                            {
                                Result.Append(", ");
                            }

                            // Attempt parameter conversion for booleans at least
                            var targetType = typeof(void);
                            if (i < targetParameters.Length)
                            {
                                targetType = targetParameters[i].ParameterType;
                            }

                            if (targetType == typeof(bool) && IsInteger(args[i], 0))
                            {
                                Result.Append("false");
                            }
                            else if (targetType == typeof(bool) && IsInteger(args[i], 1))
                            {
                                Result.Append("true");
                            }
                            // Auto convert from constant integer arguments to enums
                            else if (typeof(Enum).IsAssignableFrom(targetType)
                                     && args[i] is ConstantExpression constantArg
                                     && constantArg.Value is int constantIntArg)
                            {
                                var targetName = Enum.GetName(targetType, constantIntArg);
                                Result.Append(targetType.Name).Append(".").Append(targetName);
                            }
                            else
                            {
                                args[i].Walk(this);
                            }
                        }

                        Result.Append(')');
                        resultType = TypeMapping.FromManagedType(member.ReturnType);
                        return;
                    }
                }
            }

            // throw new NotSupportedException("Unknown method called on " + extensionType + ": " + methodName);

            target?.Walk(this);
            Result.Append('.');
            Result.Append(methodName);
            Result.Append('(');
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    Result.Append(", ");
                }

                args[i].Walk(this);
            }

            resultType = GuessedType.Unknown;
        }

        private void ConvertSpellMethod(Expression target, string methodName, Expression[] args,
            out GuessedType resultType)
        {
            // The spell "methods" get passed the spell id most of the time which is stupid,
            // so we transform calls of the form x.<method>(x.id) to just x.<method>()
            if (args.Length >= 1)
            {
                var instanceStr = NodeToString(target);
                if (args[0] is MemberExpression firstArgMemberExpression
                    && firstArgMemberExpression.Name == "id"
                    && NodeToString(firstArgMemberExpression.Target) == instanceStr)
                {
                    args = args.Skip(1).ToArray();
                }
            }

            ConvertMethodUsingExtensionClass(typeof(ScriptSpellExtensions), target, methodName, args,
                out resultType);
        }

        private void ConvertSpellTargetsMethod(Expression target, string methodName, Expression[] args,
            out GuessedType resultType)
        {
            if (!(target is MemberExpression memberExpression))
            {
                throw new NotSupportedException("Methods on spell target lists need to be called via spell.targets");
            }

            memberExpression.Target.Walk(this);
            Result.Append('.');

            // The methods on this type are actually mapped to the parent of the targets list
            if (methodName == "remove_target")
            {
                Result.Append(nameof(SpellPacketBody.RemoveTarget));
            }
            else if (methodName == "remove_list")
            {
                Result.Append(nameof(SpellPacketBody.RemoveTargets));
            }
            else
            {
                throw new NotSupportedException("Unknown method " + methodName + " called on spell target list.");
            }

            Result.Append('(');
            PrintArguments(args);
            Result.Append(')');
            resultType = GuessedType.Unknown;
        }

        public override bool Walk(ConditionalExpression node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(ConstantExpression node)
        {
            if (node.Value is string s)
            {
                // Heuristic for replacing mes\\ with mes/
                if (s.StartsWith("mes\\"))
                {
                    s = "mes/" + s.Substring("mes\\".Length);
                }

                // Roslyn can take care of the proper escapes
                var t = SyntaxFactory.Literal(s).ToFullString();
                Result.Append(t);
                _lastType = GuessedType.String;
            }
            else if (node.Value is bool boolValue)
            {
                Result.Append(boolValue ? "true" : "false");
                _lastType = GuessedType.Bool;
            }
            else if (node.Value is int intValue)
            {
                Result.Append(intValue.ToString(CultureInfo.InvariantCulture));
                _lastType = GuessedType.Integer;
            }
            else if (node.Value is double doubleValue)
            {
                Result.Append(doubleValue.ToString(CultureInfo.InvariantCulture));
                Result.Append('f');
                _lastType = GuessedType.Float;
            }
            else if (node.Value == null)
            {
                Result.Append("null");
                _lastType = GuessedType.Object;
            }
            else if (node.Value is BigInteger bigIntValue)
            {
                // This is very rarely used (only utilities.py in an unused function),
                // so we'll shoddily convert it.
                bigIntValue.AsInt64(out var longValue);
                Result.Append(longValue);
                _lastType = GuessedType.Integer;
            }
            else
            {
                throw new NotSupportedException("Unsupported value type: " + node.Type);
            }

            return false;
        }

        public override bool Walk(DictionaryComprehension node)
        {
            throw new NotSupportedException();
        }

        public void GetDictionaryTypes(DictionaryExpression node, out GuessedType keyType,
            out GuessedType valueType,
            out string managedType)
        {
            // We'll peek at the first item... otherwise we'll have to go with List<FIXME> ;-)
            keyType = GuessedType.Unknown;
            valueType = GuessedType.Unknown;

            if (node.Items.Count > 0)
            {
                keyType = GetExpressionType(node.Items[0].SliceStart);
                valueType = GetExpressionType(node.Items[0].SliceStop);
            }

            managedType = "Dictionary<" + TypeMapping.GuessManagedType(keyType) + ", "
                          + TypeMapping.GuessManagedType(valueType) + ">";
        }

        public void GetListType(ListExpression node, out GuessedType valueType, out string managedType)
        {
            // We'll peek at the first item... otherwise we'll have to go with List<FIXME> ;-)
            valueType = GuessedType.Unknown;

            if (node.Items.Count > 0)
            {
                valueType = GetExpressionType(node.Items[0]);
            }

            managedType = "List<" + TypeMapping.GuessManagedType(valueType) + ">";
        }

        public override bool Walk(ListExpression node)
        {
            if (node.Items.Count == 0)
            {
                Result.Append("new List<GameObjectBody>()");
                _lastType = GuessedType.ObjectList;
                return false;
            }

            Result.Append("new []{");
            for (var index = 0; index < node.Items.Count; index++)
            {
                if (index != 0)
                {
                    Result.Append(", ");
                }

                var item = node.Items[index];
                item.Walk(this);
            }

            Result.Append("}");
            _lastType = GuessedType.Unknown;
            return false;
        }

        public override bool Walk(DictionaryExpression node)
        {
            GetDictionaryTypes(node, out _, out _, out var dictType);

            Result.AppendLine("new " + dictType + " {");

            foreach (var item in node.Items)
            {
                if (item.SliceStep != null)
                {
                    throw new NotSupportedException();
                }

                Result.Append("{");
                item.SliceStart.Walk(this);
                Result.Append(",");
                item.SliceStop.Walk(this);
                Result.AppendLine("},");
            }

            Result.AppendLine("}");
            _lastType = GuessedType.Unknown;
            return false;
        }

        public override bool Walk(ErrorExpression node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(GeneratorExpression node)
        {
            throw new NotSupportedException();
        }

        private static bool IsPath(Expression memberExpression, params string[] parts)
        {
            var currentIdx = parts.Length - 1;
            string currentName = null;
            Expression nextTarget = memberExpression;

            void GetNext()
            {
                if (nextTarget is MemberExpression nextMemberExpression)
                {
                    currentName = nextMemberExpression.Name;
                    nextTarget = nextMemberExpression.Target;
                }
                else if (nextTarget is NameExpression nameExpression)
                {
                    currentName = nameExpression.Name;
                    nextTarget = null;
                }
                else
                {
                    currentName = null;
                    nextTarget = null;
                }
            }

            GetNext();

            while (currentIdx >= 0)
            {
                if (parts[currentIdx] != currentName)
                {
                    return false;
                }

                GetNext();

                currentIdx--;
            }

            return nextTarget == null;
        }

        private static bool IsQuestAccess(IndexExpression expression)
        {
            return IsPath(expression.Target, "game", "quests");
        }

        private static bool IsQuestStateAccess(Expression expression, out Expression questIndex)
        {
            if (expression is MemberExpression memberExpression
                && memberExpression.Name == "state"
                && memberExpression.Target is IndexExpression indexExpression
                && IsQuestAccess(indexExpression))
            {
                questIndex = indexExpression.Index;
                return true;
            }

            questIndex = null;
            return false;
        }

        private static bool IsCounterAccess(IndexExpression expression)
        {
            return IsPath(expression.Target, "game", "counters");
        }

        private static bool IsAreasAccess(IndexExpression expression)
        {
            return IsPath(expression.Target, "game", "areas");
        }

        private static bool IsGlobalVarsAccess(IndexExpression expression)
        {
            return IsPath(expression.Target, "game", "global_vars");
        }

        private static bool IsGlobalFlagsAccess(IndexExpression expression)
        {
            return IsPath(expression.Target, "game", "global_flags");
        }

        private static bool IsPartyAccess(IndexExpression expression)
        {
            return IsPath(expression.Target, "game", "party");
        }

        private GuessedType _lastType;

        public override bool Walk(IndexExpression node)
        {
            // Special casing for index expressions on the game object which are hard to generalize
            // because they should be translated to method calls.
            if (IsQuestAccess(node))
            {
                Result.Append("GetQuest(");
                node.Index.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Quest;
                return false;
            }
            else if (IsGlobalFlagsAccess(node))
            {
                Result.Append("GetGlobalFlag(");
                node.Index.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Bool;
                return false;
            }
            else if (IsGlobalVarsAccess(node))
            {
                Result.Append("GetGlobalVar(");
                node.Index.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Integer;
                return false;
            }
            else if (IsAreasAccess(node))
            {
                Result.Append("IsAreaKnown(");
                node.Index.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Bool;
                return false;
            }
            else if (IsCounterAccess(node))
            {
                Result.Append("GetCounter(");
                node.Index.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.Integer;
                return false;
            }
            else if (node.Target is MemberExpression indexMemberExpr
                     && GetExpressionType(indexMemberExpr.Target) == GuessedType.Object
                     && indexMemberExpr.Name == "scripts")
            {
                indexMemberExpr.Target.Walk(this);
                Result.Append(".GetScriptId(");
                PrintObjectEvent(node.Index);
                Result.Append(")");
                _lastType = GuessedType.Integer;
                return false;
            }
            else if (IsPartyAccess(node))
            {
                // Auto translate access to game.party[0] to PartyLeader
                if (node.Index is ConstantExpression constantExpression && constantExpression.Value?.Equals(0) == true)
                {
                    Result.Append("PartyLeader");
                }
                else
                {
                    // This is more or less deprecated and only used in Co8
                    Result.Append("GameSystems.Party.GetPartyGroupMemberN(");
                    node.Index.Walk(this);
                    Result.Append(")");
                }

                _lastType = GuessedType.Object;
                return false;
            }
            else if (GetExpressionType(node.Target) == GuessedType.RandomEncounterEnemy)
            {
                // Replace number based access to property based access for encounter enemies
                node.Target.Walk(this);
                Result.Append(".");
                if (IsInteger(node.Index, 0))
                {
                    Result.Append("ProtoId");
                    _lastType = GuessedType.Integer;
                }
                else if (IsInteger(node.Index, 1))
                {
                    Result.Append("Count");
                    _lastType = GuessedType.Integer;
                }
                else
                {
                    throw new NotSupportedException("Non-constant indexer access to random encounter enemy: " +
                                                    NodeToString(node));
                }

                return false;
            }

            node.Target.Walk(this);
            var listType = _lastType;
            Result.Append("[");
            node.Index.Walk(this);
            Result.Append("]");
            _lastType = GetListElementType(listType);

            return false;
        }

        private void PrintObjectEvent(Expression value)
        {
            if (value is ConstantExpression constantExpression
                && constantExpression.Value is int intValue)
            {
                var name = typeof(ObjScriptEvent).GetEnumName((ObjScriptEvent) intValue);
                Result.Append("ObjScriptEvent.");
                Result.Append(name);
            }
            else
            {
                value.Walk(this);
            }
        }

        internal GuessedType GetListElementType(GuessedType listType)
        {
            return listType switch
            {
                GuessedType.ObjectList => GuessedType.Object,
                GuessedType.SpellTargets => GuessedType.SpellTarget,
                GuessedType.IntList => GuessedType.Integer,
                GuessedType.TrapDamageList => GuessedType.TrapDamage,
                GuessedType.RandomEncounterEnemies => GuessedType.RandomEncounterEnemy,
                _ => GuessedType.Unknown
            };
        }

        public override bool Walk(LambdaExpression node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(ListComprehension node)
        {
            Result.Append("FIXME ");
            Result.Append(NodeToString(node));
            return false;
        }

        public override void PostWalk(ExpressionStatement node)
        {
            Result.Append(';');
        }

        public override bool Walk(MemberExpression node)
        {
            // Special translation for game.quest[1].state when being accessed for reading
            // assignments are handled in AssignmentStatement
            if (IsQuestStateAccess(node, out var questIndex))
            {
                Result.Append("GetQuestState(");
                questIndex.Walk(this);
                Result.Append(")");
                _lastType = GuessedType.QuestState;
                return false;
            }

            var targetExpressionType = GetExpressionType(node.Target);
            if (targetExpressionType == GuessedType.Object)
            {
                if (TranslateObjectProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.TrapSprungEvent)
            {
                if (TranslateTrapSprungEventProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.TrapDamage)
            {
                if (TranslateTrapDamageProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.Spell)
            {
                if (TranslateSpellProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.RandomEncounter)
            {
                if (TranslateRandomEncounterProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.RandomEncounterQuery)
            {
                if (TranslateRandomEncounterQueryProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.Dice)
            {
                if (TranslateDiceProperty(node.Target, node.Name))
                {
                    return false;
                }
            }
            else if (targetExpressionType == GuessedType.SpellTarget)
            {
                node.Target.Walk(this);
                Result.Append('.');
                switch (node.Name)
                {
                    case "obj":
                        Result.Append(nameof(SpellTarget.Object));
                        _lastType = GuessedType.Object;
                        return false;
                    case "partsys_id":
                        Result.Append(nameof(SpellTarget.ParticleSystem));
                        _lastType = GuessedType.ParticleSystem;
                        return false;
                    default:
                        break;
                }
            }
            else if (IsGameGlobal(node.Target) && TranslateGameProperty(node.Name))
            {
                return false;
            }
            else if (targetExpressionType == GuessedType.SpecialConditionIo && TranslateDispIoProperty(node))
            {
                return false;
            }
            else if (targetExpressionType == GuessedType.BonusList && TranslateBonusListProperty(node))
            {
                return false;
            }
            else if (targetExpressionType == GuessedType.D20Action && TranslateD20ActionProperty(node))
            {
                return false;
            }
            else if (targetExpressionType == GuessedType.AttackPacket)
            {
                TranslateAttackPacketProperty(node);
                return false;
            }
            else if (targetExpressionType == GuessedType.DamagePacket)
            {
                TranslateDamagePacketProperty(node);
                return false;
            }

            node.Target.Walk(this);
            Result.Append('.');
            // TODO: Translation of method names, which needs type inference for Target
            Result.Append(node.Name);
            Result.Append("/*" + _lastType + "*/");
            _lastType = GuessedType.Unknown;
            return false;
        }

        private bool TranslateDispIoProperty(MemberExpression node)
        {
            //  Special casing for translating access to the condition disp'io.
            switch (node.Name)
            {
                case "bonus_list":
                    Result.Append("dispIo.");
                    Result.Append(nameof(DispIoBonusList.bonlist));
                    _lastType = GuessedType.BonusList;
                    return true;
                case "d20a":
                    Result.Append("dispIo.");
                    Result.Append(nameof(DispIoD20ActionTurnBased.action));
                    _lastType = GuessedType.D20Action;
                    return true;
                case "arg0":
                    Result.Append("dispIo.");
                    Result.Append(nameof(EvtObjSpellCaster.arg0));
                    _lastType = GuessedType.Integer;
                    return true;
                case "attack_packet":
                    Result.Append("dispIo.");
                    Result.Append(nameof(DispIoDamage.attackPacket));
                    _lastType = GuessedType.AttackPacket;
                    return true;
                case "damage_packet":
                    Result.Append("dispIo.");
                    Result.Append(nameof(DispIoDamage.damage));
                    _lastType = GuessedType.DamagePacket;
                    return true;
                default:
                    Console.WriteLine("Unknown dispIo property: " + node.Name);
                    return false;
            }
        }

        private bool TranslateBonusListProperty(MemberExpression node)
        {
            string translatedName;
            switch (node.Name)
            {
                case "add":
                    translatedName = nameof(BonusList.AddBonus);
                    _lastType = GuessedType.Void;
                    break;
                case "add_from_feat":
                    translatedName = nameof(BonusList.AddBonusFromFeat);
                    _lastType = GuessedType.Void;
                    break;
                case "add_cap":
                    translatedName = nameof(BonusList.AddCap);
                    _lastType = GuessedType.Void;
                    break;
                default:
                    Console.WriteLine("Unknown bonuslist property: " + node.Name);
                    return false;
            }

            node.Target.Walk(this);
            Result.Append(".");
            Result.Append(translatedName);
            return true;
        }

        private bool TranslateD20ActionProperty(MemberExpression node)
        {
            string translatedName;
            switch (node.Name)
            {
                case "flags":
                    translatedName = nameof(D20Action.d20Caf);
                    _lastType = GuessedType.D20CAF;
                    break;
                case "anim_id":
                    translatedName = nameof(D20Action.animID);
                    _lastType = GuessedType.Integer;
                    break;
                case "spell_id":
                    translatedName = nameof(D20Action.spellId);
                    _lastType = GuessedType.Integer;
                    break;
                case "target":
                    translatedName = nameof(D20Action.d20ATarget);
                    _lastType = GuessedType.Object;
                    break;
                default:
                    Console.WriteLine("Unknown D20 action property: " + node.Name);
                    return false;
            }

            node.Target.Walk(this);
            Result.Append(".");
            Result.Append(translatedName);
            return true;
        }

        private void TranslateAttackPacketProperty(MemberExpression node)
        {
            node.Target.Walk(this);
            Result.Append('.');
            switch (node.Name)
            {
                case "target":
                    Result.Append(nameof(AttackPacket.victim));
                    _lastType = GuessedType.Object;
                    break;
                case "get_flags":
                    Result.Append(nameof(AttackPacket.flags));
                    _lastType = GuessedType.D20CAF;
                    break;
                case "action_type":
                    Result.Append(nameof(AttackPacket.d20ActnType));
                    _lastType = GuessedType.D20ActionType;
                    break;
                case "event_key":
                    Result.Append(nameof(AttackPacket.dispKey));
                    _lastType = GuessedType.DispatcherKey;
                    break;
                case "get_weapon_used":
                    Result.Append(nameof(AttackPacket.GetWeaponUsed));
                    _lastType = GuessedType.Void;
                    break;
                default:
                    Result.Append(node.Name);
                    Result.Append("/*AttackPacket*/");
                    _lastType = GuessedType.Unknown;
                    break;
            }
        }

        private void TranslateDamagePacketProperty(MemberExpression node)
        {
            node.Target.Walk(this);
            Result.Append('.');
            switch (node.Name)
            {
                case "bonus_list":
                    Result.Append(nameof(DamagePacket.bonuses));
                    _lastType = GuessedType.BonusList;
                    break;
                default:
                    Result.Append(node.Name);
                    Result.Append("/*DamagePacket*/");
                    _lastType = GuessedType.Unknown;
                    break;
                    ;
            }
        }

        private bool TranslateRandomEncounterProperty(Expression target, string propertyName)
        {
            target.Walk(this);
            Result.Append('.');
            switch (propertyName)
            {
                case "id":
                    Result.Append(nameof(RandomEncounter.Id));
                    _lastType = GuessedType.Integer;
                    return true;
                case "flags":
                    Result.Append(nameof(RandomEncounter.Flags));
                    _lastType = GuessedType.Integer;
                    return true;
                case "title":
                    Result.Append(nameof(RandomEncounter.Title));
                    _lastType = GuessedType.Integer;
                    return true;
                case "dc":
                    Result.Append(nameof(RandomEncounter.DC));
                    _lastType = GuessedType.Integer;
                    return true;
                case "map":
                    Result.Append(nameof(RandomEncounter.Map));
                    _lastType = GuessedType.Integer;
                    return true;
                case "enemies":
                    Result.Append(nameof(RandomEncounter.Enemies));
                    _lastType = GuessedType.RandomEncounterEnemies;
                    return true;
                case "location":
                    Result.Append(nameof(RandomEncounter.Location));
                    _lastType = GuessedType.Location;
                    return true;
                default:
                    throw new NotSupportedException($"Accessing unknown property {propertyName} on random encounter.");
            }
        }

        private bool TranslateRandomEncounterQueryProperty(Expression target, string propertyName)
        {
            target.Walk(this);
            Result.Append('.');
            switch (propertyName)
            {
                case "terrain":
                    Result.Append(nameof(RandomEncounterQuery.Terrain));
                    _lastType = GuessedType.MapTerrain;
                    return true;
                case "flags":
                    Result.Append(nameof(RandomEncounterQuery.Type));
                    _lastType = GuessedType.RandomEncounterType;
                    return true;
                default:
                    throw new NotSupportedException(
                        $"Accessing unknown property {propertyName} on random encounter query.");
            }
        }

        private bool TranslateDiceProperty(Expression target, string propertyName)
        {
            switch (propertyName)
            {
                // because a few spell scripts use this, guess it's accepted because originally it did an _strnicmp using the input length
                case "number":
                case "num":
                case "n":
                    target.Walk(this);
                    Result.Append('.');
                    Result.Append(nameof(Dice.Count));
                    _lastType = GuessedType.Integer;
                    return true;
                case "size":
                    target.Walk(this);
                    Result.Append('.');
                    Result.Append(nameof(Dice.Sides));
                    _lastType = GuessedType.Integer;
                    return true;
                case "bonus":
                    target.Walk(this);
                    Result.Append('.');
                    Result.Append(nameof(Dice.Modifier));
                    _lastType = GuessedType.Integer;
                    return true;
                default:
                    return false;
            }
        }

        private bool TranslateGameProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "global_vars":
                    throw new NotSupportedException(
                        "game.global_vars only supported in IndexExpression (game.global_vars[x])");
                case "global_flags":
                    throw new NotSupportedException(
                        "game.global_flags only supported in IndexExpression (game.global_flags[x])");
                case "quests":
                    throw new NotSupportedException("game.quests only supported in IndexExpression (game.quests[x])");
                case "leader":
                    Result.Append("SelectedPartyLeader");
                    _lastType = GuessedType.Object;
                    return true;
                case "party_alignment":
                    Result.Append("PartyAlignment");
                    _lastType = GuessedType.Alignment;
                    return true;
                case "story_state":
                    Result.Append("StoryState");
                    _lastType = GuessedType.Integer;
                    return true;
                case "areas":
                    throw new NotSupportedException("game.areas only supported in IndexExpression (game.areas[x])");
                case "sid":
                    throw new NotSupportedException("game.sid was never used.");
                case "new_sid":
                    throw new NotSupportedException("Only assignment to game.new_sid is supported.");
                case "selected":
                    Result.Append("GameSystems.Party.Selected");
                    _lastType = GuessedType.ObjectList;
                    return true;
                case "party":
                    Result.Append("GameSystems.Party.PartyMembers");
                    _lastType = GuessedType.ObjectList;
                    return true;
                case "hovered":
                    throw new NotSupportedException("game.hovered was never used.");
                case "maps_visited":
                    throw new NotSupportedException("game.maps_visited was never used.");
                case "counters":
                    throw new NotSupportedException(
                        "game.counters is only supported in index expressions (i.e. game.counters[x])");
                case "encounter_queue":
                    Result.Append("EncounterQueue");
                    _lastType = GuessedType.EncounterQueue;
                    return true;
                case "time":
                    Result.Append("CurrentTime");
                    _lastType = GuessedType.Time;
                    return true;
                case "floaters":
                    throw new NotSupportedException("game.floaters was never used.");
                case "console":
                    throw new NotSupportedException("game.console was never used.");
                default:
                    throw new NotSupportedException("Unknown game property accessed: " + propertyName);
            }
        }

        private bool TranslateTrapDamageProperty(Expression nodeTarget, string propertyName)
        {
            bool Map(string newName, GuessedType newType)
            {
                nodeTarget.Walk(this);
                Result.Append(".");
                Result.Append(newName);
                _lastType = newType;
                return true;
            }

            switch (propertyName)
            {
                case "damage":
                    return Map("Dice", GuessedType.Dice);
                case "type":
                    return Map("Type", GuessedType.DamageType);
                default:
                    throw new NotSupportedException("Unknown trap damage property accessed: " + propertyName);
            }
        }

        private bool TranslateTrapSprungEventProperty(Expression nodeTarget, string propertyName)
        {
            bool Map(string newName, GuessedType newType)
            {
                nodeTarget.Walk(this);
                Result.Append(".");
                Result.Append(newName);
                _lastType = newType;
                return true;
            }

            switch (propertyName)
            {
                case "id":
                    return Map("Type.Id", GuessedType.Integer);
                case "obj":
                    return Map("Object", GuessedType.Object);
                case "partsys":
                    return Map("Type.ParticleSystemId", GuessedType.String);
                case "damage":
                    return Map("Type.Damage", GuessedType.TrapDamageList);
                default:
                    throw new NotSupportedException("Unknown trap sprung event property accessed: " + propertyName);
            }
        }

        private bool TranslateObjectProperty(Expression nodeTarget, string propertyName)
        {
            bool Map(string newName, GuessedType newType)
            {
                nodeTarget.Walk(this);
                Result.Append(".");
                Result.Append(newName);
                _lastType = newType;
                return true;
            }

            switch (propertyName)
            {
                case "proto":
                    return Map(nameof(GameObjectBody.ProtoId), GuessedType.Integer);
                case "area":
                    return Map("GetArea()", GuessedType.Integer);
                case "name":
                    return Map("GetNameId()", GuessedType.Integer);
                case "type":
                    return Map(nameof(GameObjectBody.type), GuessedType.ObjectType);
                case "location":
                    return Map(nameof(GameObjectBody.GetLocation) + "()", GuessedType.Unknown);
                case "radius":
                    return Map("GetRadius()", GuessedType.Float);
                case "height":
                    return Map("GetRenderHeight()", GuessedType.Float);
                case "rotation":
                    return Map(nameof(GameObjectBody.Rotation), GuessedType.Float);
                case "hit_dice":
                    return Map("GetHitDice()", GuessedType.Integer);
                case "hit_dice_num":
                    Result.Append("GameSystems.Critter.GetHitDiceNum(");
                    nodeTarget.Walk(this);
                    Result.Append(")");
                    _lastType = GuessedType.Integer;
                    return true;
                case "get_hp_cur":
                    throw new NotSupportedException("obj.get_hp_cur was never used");
                case "get_hp_max":
                    throw new NotSupportedException("obj.get_hp_max was never used");
                case "get_size":
                    Result.Append("GameSystems.Stat.DispatchGetSizeCategory(");
                    nodeTarget.Walk(this);
                    Result.Append(")");
                    _lastType = GuessedType.Integer;
                    return true;
                case "off_x":
                    return Map(nameof(GameObjectBody.OffsetX), GuessedType.Float);
                case "off_y":
                    return Map(nameof(GameObjectBody.OffsetY), GuessedType.Float);
                case "map":
                    return Map("GetMap()", GuessedType.Integer);
                case "scripts":
                    throw new NotSupportedException("obj.scripts should only be used in index expressions.");
                case "origin":
                    return Map("GetInt32(obj_f.critter_teleport_map)", GuessedType.Integer);
                case "substitute_inventory":
                    return Map("GetObject(obj_f.npc_substitute_inventory)", GuessedType.Object);
                case "feats":
                    return Map("EnumerateFeats()", GuessedType.Unknown);
                case "loots":
                    return Map("GetLootSharingType()", GuessedType.LootSharingType);
                default:
                    Console.WriteLine("Unknown object property accessed: " + propertyName);
                    // throw new NotSupportedException("Unknown object property accessed: " + propertyName);
                    return Map(propertyName, GuessedType.Unknown);
            }
        }

        private bool TranslateSpellProperty(Expression nodeTarget, string propertyName)
        {
            bool Map(string newName, GuessedType newType)
            {
                nodeTarget.Walk(this);
                Result.Append(".");
                Result.Append(newName);
                _lastType = newType;
                return true;
            }

            switch (propertyName)
            {
                case "begin_round_obj":
                    throw new NotSupportedException("TODO Introduce new param to OnBeginRound and use it here.");
                case "caster":
                    return Map(nameof(SpellPacketBody.caster), GuessedType.Object);
                case "caster_class":
                    return Map(nameof(SpellPacketBody.spellClass), GuessedType.Integer);
                case "spell_level":
                    return Map(nameof(SpellPacketBody.spellKnownSlotLevel), GuessedType.Integer);
                case "range_exact":
                    return Map(nameof(SpellPacketBody.spellRange), GuessedType.Integer);
                case "target_loc":
                    // NOTE: In almost all cases, aoecenter is used in a context where the full location is desired
                    return Map(nameof(SpellPacketBody.aoeCenter), GuessedType.LocationFull);
                case "target_loc_off_x":
                    return Map(nameof(SpellPacketBody.aoeCenter) + "." + nameof(LocAndOffsets.off_x),
                        GuessedType.Float);
                case "target_loc_off_y":
                    return Map(nameof(SpellPacketBody.aoeCenter) + "." + nameof(LocAndOffsets.off_y),
                        GuessedType.Float);
                case "target_loc_off_z":
                    return Map(nameof(SpellPacketBody.aoeCenterZ), GuessedType.Float);
                case "target_loc_full":
                    return Map(nameof(SpellPacketBody.aoeCenter), GuessedType.LocationFull);
                case "caster_level":
                    return Map(nameof(SpellPacketBody.casterLevel), GuessedType.Integer);
                case "dc":
                    return Map(nameof(SpellPacketBody.dc), GuessedType.Integer);
                case "id":
                    return Map(nameof(SpellPacketBody.spellId), GuessedType.Integer);
                case "duration":
                    return Map(nameof(SpellPacketBody.duration), GuessedType.Integer);
                case "duration_remaining":
                    return Map(nameof(SpellPacketBody.durationRemaining), GuessedType.Integer);
                case "num_of_targets":
                    return Map(nameof(SpellPacketBody.Targets) + ".Length", GuessedType.Integer);
                case "num_of_projectiles":
                    return Map(nameof(SpellPacketBody.projectiles) + ".Length", GuessedType.Integer);
                case "caster_partsys_id":
                    return Map(nameof(SpellPacketBody.casterPartSys), GuessedType.Integer);
                case "target_list":
                    return Map(nameof(SpellPacketBody.Targets), GuessedType.SpellTargets);
                case "spell":
                    return Map(nameof(SpellPacketBody.spellEnum), GuessedType.Integer);
                default:
                    return false;
            }
        }

        // Attempt to coerce an expression to a boolean type
        private bool IsBoolean(Expression expression, bool test)
        {
            if (expression is ConstantExpression constantExpression
                && constantExpression.Value is bool)
            {
                return (bool) constantExpression.Value == test;
            }

            return false;
        }

        private bool IsInteger(Expression expression, int number)
        {
            if (expression is ConstantExpression constantExpression
                && constantExpression.Value is int intValue)
            {
                return intValue == number;
            }

            return false;
        }

        private bool TryGetStringConstant(Expression expression, out string constant)
        {
            if (expression is ConstantExpression constantExpression
                && constantExpression.Value is string stringConstant)
            {
                constant = stringConstant;
                return true;
            }

            constant = null;
            return false;
        }

        /// <summary>
        /// Converts 1 and 0 to True and False in a boolean context.
        /// </summary>
        private Expression CoerceToBoolean(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
            {
                if (constantExpression.Value is int intValue)
                {
                    if (intValue == 1)
                    {
                        return new ConstantExpression(true);
                    }
                    else if (intValue == 0)
                    {
                        return new ConstantExpression(false);
                    }
                }
            }

            return expression;
        }

        public override bool Walk(NameExpression node)
        {
            var name = node.Name;

            if (_variables.ContainsKey(name))
            {
                // Special translation for condition attachee
                if (_variables[name] == GuessedType.SpecialConditionAttachee)
                {
                    Result.Append("evt.");
                    Result.Append(nameof(DispatcherCallbackArgs.objHndCaller));
                    _lastType = GuessedType.Object;
                    return false;
                }

                Result.Append(name);
                _lastType = _variables[name];
                return false;
            }

            if (PythonConstants.Constants.TryGetValue(name, out var translatedConstant))
            {
                Result.Append(translatedConstant);
                _lastType = PythonConstants.ConstantTypes[name];
                return false;
            }

            Result.Append(name); // Needs name translation
            _lastType = GuessedType.Unknown;
            return true;
        }

        private static bool IsGameGlobal(Expression expression)
        {
            return expression is NameExpression nameExpression
                   && nameExpression.Name == "game";
        }

        public override bool Walk(OrExpression node)
        {
            node.Left.Walk(this);
            Result.Append(" || ");
            node.Right.Walk(this);
            return false;
        }

        public override bool Walk(ParenthesisExpression node)
        {
            Result.Append("(");
            node.Expression.Walk(this);
            Result.Append(")");
            return false;
        }

        public override bool Walk(SetComprehension node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(SetExpression node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(SliceExpression node)
        {
            if (node.StepProvided)
            {
                throw new NotSupportedException("Slices with step are not supported");
            }

            node.SliceStart?.Walk(this);
            Result.Append("..");
            node.SliceStop?.Walk(this);

            return false;
        }

        public override bool Walk(TupleExpression node)
        {
            Result.Append("(");
            for (var i = 0; i < node.Items.Count; i++)
            {
                if (i > 0)
                {
                    Result.Append(", ");
                }

                node.Items[i].Walk(this);
            }

            Result.Append(")");
            return false;
        }

        private Expression UnwrapParenthesis(Expression expr)
        {
            if (expr is ParenthesisExpression p)
            {
                return UnwrapParenthesis(p.Expression);
            }

            return expr;
        }

        public override bool Walk(UnaryExpression node)
        {
            // Fix expressions like: !(bitField  & mask) and convert them into (bitField & mask) == 0
            if (node.Op == PythonOperator.Not
                && UnwrapParenthesis(node.Expression) is BinaryExpression binaryExpression
                && binaryExpression.Operator == PythonOperator.BitwiseAnd)
            {
                var replacement = new BinaryExpression(PythonOperator.Equal,
                    new ParenthesisExpression(binaryExpression),
                    new ConstantExpression(0));
                replacement.Walk(this);
                return false;
            }

            Result.Append(GetOperatorSymbol(node.Op));
            // This is a bit fishy and imprecise, but it's a quick fix
            if (node.Expression is BinaryExpression)
            {
                Result.Append('(');
                node.Expression.Walk(this);
                Result.Append(')');
            }
            else
            {
                node.Expression.Walk(this);
            }

            return false;
        }

        public override bool Walk(YieldExpression node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(AssertStatement node)
        {
            Result.Append("Trace.Assert(");
            node.Test.Walk(this);
            if (node.Message != null)
            {
                Result.Append(", ");
                node.Message.Walk(this);
            }

            Result.Append(");");
            return false;
        }

        private bool IsScriptChange(AssignmentStatement assignment, out Expression newScript)
        {
            if (assignment.Left.Count == 1
                && assignment.Left[0] is MemberExpression memberExpression
                && memberExpression.Name == "new_sid"
                && memberExpression.Target is NameExpression targetName
                && targetName.Name == "game")
            {
                newScript = assignment.Right;
                return true;
            }

            newScript = null;
            return false;
        }

        public override bool Walk(AssignmentStatement node)
        {
            // Special casing for "game.new_sid = 0;", which needs to be replaced with "DetachScript();"
            if (IsScriptChange(node, out var newScript))
            {
                if (IsInteger(newScript, 0))
                {
                    Result.Append("DetachScript();");
                }
                else
                {
                    Result.Append("ReplaceCurrentScript(");
                    newScript.Walk(this);
                    Result.Append(");");
                }

                _lastType = GuessedType.Unknown;
                return false;
            }

            string firstVariable = null;
            foreach (var expression in node.Left)
            {
                // Special cases for assignment to vars/flags
                if (expression is IndexExpression indexExpression)
                {
                    if (IsGlobalVarsAccess(indexExpression))
                    {
                        Result.Append("SetGlobalVar(");
                        indexExpression.Index.Walk(this);
                        Result.Append(", ");
                        node.Right.Walk(this);
                        Result.Append(");");
                        _lastType = GuessedType.Unknown;
                        return false;
                    }
                    else if (IsGlobalFlagsAccess(indexExpression))
                    {
                        Result.Append("SetGlobalFlag(");
                        indexExpression.Index.Walk(this);
                        Result.Append(", ");
                        CoerceToBoolean(node.Right).Walk(this);
                        Result.Append(");");
                        _lastType = GuessedType.Unknown;
                        return false;
                    }
                    else if (IsCounterAccess(indexExpression))
                    {
                        Result.Append("SetCounter(");
                        indexExpression.Index.Walk(this);
                        Result.Append(", ");
                        node.Right.Walk(this);
                        Result.Append(");");
                        _lastType = GuessedType.Unknown;
                        return false;
                    }
                    else if (IsAreasAccess(indexExpression))
                    {
                        // RHS Must be a truthy value!
                        if (!(CoerceToBoolean(node.Right) is ConstantExpression constantExpression)
                            || !constantExpression.Value.Equals(true))
                        {
                            throw new NotSupportedException(
                                "Setter access to game.areas is only allowed with a 'true' value, since anything else was a noop in Vanilla.");
                        }

                        Result.Append("MakeAreaKnown(");
                        indexExpression.Index.Walk(this);
                        Result.Append(");");
                        _lastType = GuessedType.Unknown;
                        return false;
                    }
                    else if (indexExpression.Target is MemberExpression memberExpression
                             && GetExpressionType(memberExpression.Target) == GuessedType.Object
                             && memberExpression.Name == "scripts")
                    {
                        memberExpression.Target.Walk(this);
                        if (IsInteger(node.Right, 0))
                        {
                            Result.Append(".RemoveScript(");
                            PrintObjectEvent(indexExpression.Index);
                        }
                        else
                        {
                            Result.Append(".SetScriptId(");
                            PrintObjectEvent(indexExpression.Index);
                            Result.Append(", ");
                            node.Right.Walk(this);
                        }

                        Result.Append(");");

                        _lastType = GuessedType.Unknown;
                        return false;
                    }
                }
                else if (IsQuestStateAccess(expression, out var questIndex))
                {
                    Result.Append("SetQuestState(");
                    questIndex.Walk(this);
                    Result.Append(", ");
                    node.Right.Walk(this);
                    Result.Append(");");
                    _lastType = GuessedType.Unknown;
                    return false;
                }
                // Handle assignment to loot sharing type, which is now an extension method
                else if (expression is MemberExpression memberExpression)
                {
                    var targetType = GetExpressionType(memberExpression.Target);
                    // Assigning tuples is not great to map
                    if (targetType == GuessedType.RandomEncounter
                        && memberExpression.Name == "enemies"
                        && node.Right is TupleExpression enemiesTuple)
                    {
                        for (var index = 0; index < enemiesTuple.Items.Count; index++)
                        {
                            var enemyItem = enemiesTuple.Items[index];
                            memberExpression.Target.Walk(this);
                            Result.Append(".").Append(nameof(RandomEncounter.AddEnemies)).Append("(");
                            if (enemyItem is TupleExpression itemTuple && itemTuple.Items.Count == 2)
                            {
                                itemTuple.Items[0].Walk(this);
                                Result.Append(", ");
                                itemTuple.Items[1].Walk(this);
                            }
                            else
                            {
                                enemyItem.Walk(this);
                                Result.Append("/*FIXMEENEMY*/");
                            }

                            Result.Append(");");
                            if (index + 1 < enemiesTuple.Items.Count)
                            {
                                Result.AppendLine(); // Newlines not for the last one...
                            }
                        }

                        return false;
                    }
                    else if (targetType == GuessedType.Object && memberExpression.Name == "loots")
                    {
                        memberExpression.Target.Walk(this);
                        Result.Append(".");
                        Result.Append(nameof(ScriptObjectExtensions.SetLootSharingType));
                        Result.Append("(");
                        node.Right.Walk(this);
                        Result.Append(");");
                        return false;
                    }
                    // Handle assignment to dice methods which require us to create new dice (since they are immutable)
                    else if (targetType == GuessedType.Dice)
                    {
                        // All assignments to Dice properties are replaced with assignments that make use of
                        // the immutable factory methods.
                        memberExpression.Target.Walk(this);
                        Result.Append(" = ");
                        memberExpression.Target.Walk(this);
                        switch (memberExpression.Name)
                        {
                            case "bonus":
                                Result.Append(".WithModifier(");
                                break;
                            case "number":
                            case "num":
                            case "n":
                                Result.Append(".WithCount(");
                                break;
                            case "size":
                                Result.Append(".WithSides(");
                                break;
                            default:
                                throw new NotSupportedException("Cannot assign to unknown property " +
                                                                $"{memberExpression.Name} of Dice.");
                        }

                        node.Right.Walk(this);
                        Result.Append(");");
                        _lastType = GuessedType.Dice;
                        return false;
                    }
                }

                string assignedVariableName = null;
                if (expression is NameExpression nameExpression)
                {
                    assignedVariableName = nameExpression.Name;
                    if (!_variables.ContainsKey(assignedVariableName))
                    {
                        // If the RHS is a null handle, we need to specify the type
                        if (NodeToString(node.Right) == "OBJ_HANDLE_NULL")
                        {
                            Result.Append(typeof(GameObjectBody).Name).Append(' ');
                        }
                        else
                        {
                            Result.Append("var ");
                        }

                        _variables[assignedVariableName] = GuessedType.Unknown;
                    }
                }

                // Handle destructuring expressions (mostly used for (x, y) = loc)
                if (expression is TupleExpression tupleExpression)
                {
                    // Make it a single var at the front if all destructured variables are unknown
                    var allVarsAreNew = tupleExpression.Items.Where(t => t is NameExpression)
                        .Cast<NameExpression>()
                        .Select(ne => ne.Name)
                        .All(vn => !_variables.ContainsKey(vn));
                    if (allVarsAreNew)
                    {
                        Result.Append("var ");
                    }

                    Result.Append("(");
                    for (var index = 0; index < tupleExpression.Items.Count; index++)
                    {
                        if (index > 0)
                        {
                            Result.Append(", ");
                        }

                        var tupleItem = tupleExpression.Items[index];
                        if (!(tupleItem is NameExpression tupleItemNameExpression))
                        {
                            tupleItem.Walk(this);
                            continue;
                        }

                        var destructuredName = tupleItemNameExpression.Name;
                        if (!_variables.ContainsKey(destructuredName))
                        {
                            if (!allVarsAreNew)
                            {
                                Result.Append("var ");
                            }

                            _variables[destructuredName] = GuessedType.Unknown;
                        }

                        Result.Append(destructuredName);
                    }

                    Result.Append(")");
                }
                else
                {
                    expression.Walk(this);
                }

                Result.Append(" = ");
                // Try reusing the first variable we assigned sth to because the RHS might have side effects
                // This only applies to statements such as "x = y = DoSomething()"
                if (firstVariable != null)
                {
                    Result.Append(firstVariable);
                }
                else
                {
                    // Heuristic based on variable name for the type of list
                    if (node.Right is ListExpression listExpression
                        && listExpression.Items.Count == 0
                        && assignedVariableName == "re_list")
                    {
                        Result.Append("new List<object>()");
                        _lastType = GuessedType.UnknownList;
                    }
                    else
                    {
                        node.Right.Walk(this);
                    }
                }

                Result.Append(";");

                if (assignedVariableName != null)
                {
                    _variables[assignedVariableName] = _lastType;
                }

                // Remember the first *actual* variable we assigned the RHS to
                if (assignedVariableName != null && firstVariable == null)
                {
                    firstVariable = assignedVariableName;
                }
            }

            _lastType = GuessedType.Unknown;
            return false;
        }

        public override bool Walk(AugmentedAssignStatement node)
        {
            // This cannot declare new variables since it's += or similar
            node.Left.Walk(this);
            Result.Append(' ');
            Result.Append(GetOperatorSymbol(node.Operator));
            Result.Append("= ");
            node.Right.Walk(this);
            Result.Append(";");
            return false;
        }

        public override bool Walk(BreakStatement node)
        {
            Result.AppendLine("break;");
            return false;
        }

        public override bool Walk(ClassDefinition node)
        {
            if (node.Decorators != null)
            {
                foreach (var nodeDecorator in node.Decorators)
                {
                    nodeDecorator.Walk(this);
                }
            }

            Result.Append("private class ");
            Result.Append(node.Name);
            if (node.Bases != null && node.Bases.Count > 0)
            {
                Result.Append(" : ");
                for (var index = 0; index < node.Bases.Count; index++)
                {
                    var nodeBase = node.Bases[index];
                    if (index > 0)
                    {
                        Result.Append(", ");
                        nodeBase.Walk(this);
                    }
                }
            }

            Result.AppendLine(" {");
            node.Body.Walk(this);
            Result.AppendLine("}");
            return false;
        }

        public override bool Walk(ContinueStatement node)
        {
            Result.AppendLine("continue;");
            return false;
        }

        public override bool Walk(DelStatement node)
        {
            foreach (var expression in node.Expressions)
            {
                var type = GetExpressionType(expression);
                switch (type)
                {
                    case GuessedType.EncounterQueue:
                        break;
                    default:
                        Result.Append("FIXME:DEL ");
                        expression.Walk(this);
                        break;
                }

                Result.AppendLine(";");
            }

            return false;
        }

        public override bool Walk(EmptyStatement node)
        {
            return false;
        }

        public override bool Walk(ExecStatement node)
        {
            throw new NotSupportedException();
        }

        private static bool IsRangeCall(Expression expression, out Expression from, out Expression to)
        {
            from = null;
            to = null;
            if (!(expression is CallExpression callExpression)
                || !(callExpression.Target is NameExpression callNameExpression)
                || callNameExpression.Name != "range" && callNameExpression.Name != "xrange")
            {
                return false;
            }

            var args = callExpression.Args;
            if (args.Count == 1)
            {
                Trace.Assert(args[0].Name == null);
                from = new ConstantExpression(0);
                to = args[0].Expression;
                return true;
            }
            else if (args.Count == 2)
            {
                Trace.Assert(args[0].Name == null);
                Trace.Assert(args[1].Name == null);
                from = args[0].Expression;
                to = args[1].Expression;
                return true;
            }
            else
            {
                return false; // We only support simple versions of the range expression
            }
        }

        public override bool Walk(ForStatement node)
        {
            if (node.Else != null)
            {
                Result.AppendLine("FIXME:FORELSE");
                Result.AppendLine("{");
                node.Else.Walk(this);
                Result.AppendLine("}");
                Result.AppendLine("FIXME:FORELSE");
            }

            // Determine the variable names that are going to be defined within the for loop
            bool needsFixme = false;
            var variableNames = new List<string>();
            if (node.Left is NameExpression nameExpression)
            {
                variableNames.Add(nameExpression.Name);
            }
            else if (node.Left is TupleExpression tupleExpression)
            {
                foreach (var tupleItem in tupleExpression.Items)
                {
                    if (tupleItem is NameExpression itemNameExpression)
                    {
                        variableNames.Add(itemNameExpression.Name);
                    }
                    else
                    {
                        needsFixme = true;
                    }
                }
            }
            else
            {
                needsFixme = true;
            }

            if (needsFixme)
            {
                Result.AppendLine("// FIXME: " + NodeToString(node.Left));
            }

            // Transform into an indexed for-loop if the RHS is a range() expression
            if (IsRangeCall(node.List, out var rangeFromExpr, out var rangeToExpr))
            {
                if (!(node.Left is NameExpression forVarNameExpr))
                {
                    throw new NotSupportedException("Unpacking the result from a range iteration wont work.");
                }

                var variableName = forVarNameExpr.Name;
                bool localScopeOfIterator = false;
                if (!_variables.ContainsKey(variableName))
                {
                    localScopeOfIterator = true;
                    _variables[variableName] = GuessedType.Integer;
                }

                Result.Append("for (var ");
                Result.Append(variableName);
                Result.Append(" = ");
                rangeFromExpr.Walk(this);
                Result.Append("; ");
                Result.Append(variableName);
                Result.Append(" < ");
                rangeToExpr.Walk(this);
                Result.Append("; ");
                Result.Append(variableName);
                Result.AppendLine("++)");
                Result.AppendLine("{");


                node.Body.Walk(this);
                Result.AppendLine("}");

                if (localScopeOfIterator)
                {
                    _variables.Remove(variableName);
                }

                return false;
            }

            var listType = GetExpressionType(node.List);
            for (var i = 0; i < variableNames.Count; i++)
            {
                if (!_variables.ContainsKey(variableNames[i]))
                {
                    // This is the most common type
                    _variables[variableNames[i]] = GetListElementType(listType);
                }
                else
                {
                    variableNames[i] = null;
                }
            }

            Result.Append("foreach (var ");
            node.Left.Walk(this);
            Result.Append(" in ");

            // Another unfortunate special case... if node.Left is a TupleExpression, and node.Items is a method call
            // to ".items()", unwrap the method call because this'll work out of the box in C#
            if (node.Left is TupleExpression te && te.Items.Count == 2
                                                && node.List is CallExpression callExpr &&
                                                callExpr.Target is MemberExpression me && me.Name == "items")
            {
                me.Target.Walk(this);
            }
            else
            {
                node.List.Walk(this);
            }

            Result.AppendLine(") {");
            node.Body.Walk(this);
            Result.AppendLine("}");

            foreach (var variableName in variableNames)
            {
                if (variableName != null)
                {
                    _variables.Remove(variableName);
                }
            }

            return false;
        }

        private string NodeToString(Node node)
        {
            return _currentScript.Content.Substring(node.StartIndex, node.EndIndex - node.StartIndex);
        }

        public override bool Walk(FromImportStatement node)
        {
            Result.Append(NodeToString(node));
            return false;
        }

        // This is used within classes
        public override bool Walk(FunctionDefinition node)
        {
            Result.Append("public FIXME ");
            Result.Append(node.Name);
            Result.Append("(");
            for (var index = 0; index < node.Parameters.Count; index++)
            {
                if (index > 0)
                {
                    Result.Append(", ");
                }

                var nodeParameter = node.Parameters[index];
                nodeParameter.Walk(this);
            }

            Result.AppendLine(")");
            Result.AppendLine("{");
            node.Body.Walk(this);
            Result.AppendLine("}");
            return false;
        }

        public override bool Walk(GlobalStatement node)
        {
            Result.Append("FIXME:GLOBAL ");
            Result.Append(string.Join(", ", node.Names));
            Result.AppendLine(";");
            return false;
        }

        private List<ISet<String>> _variableStack = new List<ISet<string>>();

        private static readonly string[] ImplicitModules =
        {
            "utilities",
            "py00439script_daemon",
            "scripts"
        };

        private void PushVariableScope()
        {
            _variableStack.Add(new HashSet<string>(_variables.Keys));
        }

        private void PopVariableScope()
        {
            var previousScope = _variableStack[^1];
            _variableStack.RemoveAt(_variableStack.Count - 1);
            var varsToRemove = _variables.Keys.Except(previousScope).ToArray();
            foreach (var varName in varsToRemove)
            {
                _variables.Remove(varName);
            }
        }

        public override bool Walk(IfStatement node)
        {
            HandleComments(node);

            // We need to move any variable declarations to the front of the IF statement if they are
            // going to be used in code after the if statement itself
            var referencedAfter = new ReferencedAfterNameWalker(node);
            CurrentFunction?.Walk(referencedAfter);
            var newDeclaredVars = new FindVariableDeclarations(this);
            foreach (var testNode in node.Tests)
            {
                testNode.Walk(newDeclaredVars);
            }

            node.ElseStatement?.Walk(newDeclaredVars);
            // Now we know every variable being declared somewhere in the if statement
            foreach (var kvp in newDeclaredVars.NewVariables)
            {
                // If such a variable is used AFTER the if statement, we pull up the declaration here
                if (referencedAfter.ReferencedNames.Contains(kvp.Key))
                {
                    var managedType = TypeMapping.GuessManagedType(kvp.Value);
                    Result.Append(managedType);
                    Result.Append(" ");
                    Result.Append(kvp.Key);
                    Result.AppendLine("; // DECL_PULL_UP");
                    _variables[kvp.Key] = kvp.Value;
                }
            }

            for (var index = 0; index < node.Tests.Count; index++)
            {
                var ifStatementTest = node.Tests[index];
                HandleComments(ifStatementTest);
                if (index > 0)
                {
                    Result.Append("else ");
                }

                Result.Append("if (");
                FixBitfieldComparison(ifStatementTest.Test).Walk(this);
                Result.Append(")");

                if (!AppendRestOfLineComment(ifStatementTest.Test.EndIndex, true))
                {
                    Result.AppendLine();
                }

                Result.AppendLine("{");
                PushVariableScope();
                ifStatementTest.Body.Walk(this);
                PopVariableScope();
                Result.AppendLine("}");
            }

            if (node.ElseStatement != null)
            {
                Result.AppendLine("else {");
                PushVariableScope();
                node.ElseStatement.Walk(this);
                PopVariableScope();
                Result.AppendLine("}");
            }

            return false;
        }

        public override bool Walk(ImportStatement node)
        {
            // This will cause a local import, we don't know how to handle this yet
            Result.Append(NodeToString(node));
            return false;
        }

        public override bool Walk(PrintStatement node)
        {
            if (node.Destination != null)
            {
                throw new NotSupportedException();
            }

            if (node.TrailingComma)
            {
                throw new NotSupportedException();
            }

            // Convert into call to logger instead, but concatenating all string literals and joining them
            // with placeholders
            var args = new List<Expression>();
            Result.Append("Logger.Info(\"");
            foreach (var expression in node.Expressions)
            {
                if (expression is ConstantExpression constantExpression &&
                    constantExpression.Value is string stringValue)
                {
                    Result.Append(stringValue.Replace("\"", "\\\""));
                }
                else
                {
                    Result.Append("{").Append(args.Count).Append("}");
                    args.Add(expression);
                }
            }

            Result.Append("\"");
            foreach (var arg in args)
            {
                Result.Append(", ");
                arg.Walk(this);
            }

            Result.Append(");");
            return false;
        }

        public override bool Walk(PythonAst node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(RaiseStatement node)
        {
            Result.Append("throw new Exception();");
            Result.Append(" /*");
            node.Value?.Walk(this);
            node.ExceptType?.Walk(this);
            node.Traceback?.Walk(this);
            Result.Append("*/");
            return false;
        }

        public override bool Walk(ReturnStatement node)
        {
            Result.Append("return");
            if (node.Expression != null)
            {
                Result.Append(" ");

                // Coerce integers to booleans if we know the function has to return a boolean
                if (ReturnType.HasValue && ReturnType.Value == GuessedType.Bool)
                {
                    CoerceToBoolean(node.Expression).Walk(this);
                }
                else
                {
                    node.Expression.Walk(this);
                    ReturnType = _lastType;
                }
            }
            else
            {
                ReturnType = null;
            }

            Result.Append(";");
            return false;
        }

        private void AppendComments(int startIndex, int endIndex)
        {
            var commentLines = _currentScript.Content.Substring(startIndex, endIndex - startIndex)
                .Split('\n')
                .Where(l => l.Contains('#'))
                .Select(l => l.Trim().TrimStart('#').TrimStart())
                .Where(l => l.Length > 0)
                .ToImmutableArray();

            if (commentLines.Length == 0)
            {
                return;
            }

            Result.AppendLine(string.Join("\n", commentLines.Select(l => "// " + l)));
        }

        private void HandleComments(Node node)
        {
            HandleComments(node.StartIndex);
        }

        private void HandleComments(int until)
        {
            if (_processComments && until > _endOfLastNode)
            {
                AppendComments(_endOfLastNode, until);
            }

            _endOfLastNode = until;
        }

        public override bool Walk(SuiteStatement node)
        {
            HandleComments(node);

            if (node.Documentation != null)
            {
                Result.AppendLine("/* " + node.Documentation + " */");
            }

            foreach (var nodeStatement in node.Statements)
            {
                HandleComments(nodeStatement);

                nodeStatement.Walk(this);

                // Is there a comment at the end of the line we could miss???
                if (!AppendRestOfLineComment(nodeStatement.EndIndex))
                {
                    Result.AppendLine();
                }
            }

            return false;
        }

        // ignoreFirstColon is needed to skip the ":" at the end of if-statement tests, because it's not part of the span
        private bool AppendRestOfLineComment(int statementEndIndex, bool ignoreFirstColon = false)
        {
            var content = _currentScript.Content;
            var idx = statementEndIndex;
            var foundComment = false;
            var comment = new StringBuilder();
            var colonEncountered = false;
            while (idx < content.Length)
            {
                var ch = content[idx++];
                if (ch == '\n')
                {
                    break;
                }
                else if (ch == '#' && !foundComment)
                {
                    foundComment = true;
                }
                else if (foundComment)
                {
                    comment.Append(ch);
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    if (ignoreFirstColon && !colonEncountered && ch == ':')
                    {
                        colonEncountered = true;
                        continue;
                    }

                    return false; // Some trailing code here
                }
            }

            _endOfLastNode = idx;
            var actualComment = comment.ToString().TrimStart().TrimStart('#').Trim();
            if (actualComment.Length > 0)
            {
                Result.Append(" // ");
                Result.Append(actualComment);
                Result.AppendLine();
                return true;
            }

            return false;
        }

        public override bool Walk(TryStatement node)
        {
            Result.AppendLine("try");
            Result.AppendLine("{");
            node.Body.Walk(this);
            Result.AppendLine("}");
            if (node.Else != null)
            {
                throw new NotSupportedException("try-else is not supported");
            }

            foreach (var handler in node.Handlers ?? Enumerable.Empty<TryStatementHandler>())
            {
                Result.Append("catch (");
                if (handler.Test != null)
                {
                    handler.Test.Walk(this);
                }
                else
                {
                    Result.Append("Exception");
                }

                Result.Append(" ");
                if (handler.Target != null)
                {
                    handler.Target.Walk(this);
                }
                else
                {
                    Result.Append("e");
                }

                Result.AppendLine(")");
                Result.AppendLine("{");
                handler.Body.Walk(this);
                Result.AppendLine("}");
            }

            if (node.Finally != null)
            {
                Result.AppendLine("finally");
                Result.AppendLine("{");
                node.Finally.Walk(this);
                Result.AppendLine("}");
            }

            _lastType = GuessedType.Unknown;
            return false;
        }

        public override bool Walk(WhileStatement node)
        {
            if (node.ElseStatement != null)
            {
                throw new NotSupportedException();
            }

            Result.Append("while (");
            node.Test.Walk(this);
            Result.AppendLine(")");
            Result.AppendLine("{");
            node.Body.Walk(this);
            Result.AppendLine("}");
            return false;
        }

        public override bool Walk(WithStatement node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(Arg node)
        {
            if (node.Name != null)
            {
                throw new NotSupportedException("Named arguments cannot be converted: " + node.Name);
            }

            return base.Walk(node);
        }

        public override bool Walk(ComprehensionFor node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(ComprehensionIf node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(DottedName node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(IfStatementTest node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(ModuleName node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(Parameter node)
        {
            Result.Append(TypeMapping.GuessTypeFromName(node.Name, ScriptType.Module));
            Result.Append(" ");
            Result.Append(node.Name);
            return false;
        }

        public override bool Walk(RelativeModuleName node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(SublistParameter node)
        {
            throw new NotSupportedException();
        }

        public override bool Walk(TryStatementHandler node)
        {
            throw new NotSupportedException();
        }

        public bool IsVariableDefined(string variableName)
        {
            return _variables.ContainsKey(variableName);
        }

        public ExpressionConverter Clone()
        {
            var result = new ExpressionConverter(_currentScript, _typings, _modules);
            result.AddVariables(_variables);
            result.QualifyCurrentScriptName = QualifyCurrentScriptName;
            return result;
        }

        public void ConvertFunction(FunctionDefinition functionDefinition)
        {
            _endOfLastNode = functionDefinition.StartIndex;
            _processComments = true;
            functionDefinition.Body.Walk(this);
        }
    }
}