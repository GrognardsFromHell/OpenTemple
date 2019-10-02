using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;

namespace ScriptConversion
{
    internal class ScriptConverter
    {
        private static readonly Dictionary<string, string> EventFunctionNameMapping = new Dictionary<string, string>
        {
            {"san_use", "OnUse"},
            {"san_destroy", "OnDestroy"},
            {"san_unlock", "OnUnlock"},
            {"san_get", "OnGet"},
            {"san_dialog", "OnDialog"},
            {"san_first_heartbeat", "OnFirstHeartbeat"},
            {"san_dying", "OnDying"},
            {"san_enter_combat", "OnEnterCombat"},
            {"san_exit_combat", "OnExitCombat"},
            {"san_start_combat", "OnStartCombat"},
            {"san_end_combat", "OnEndCombat"},
            {"san_heartbeat", "OnHeartbeat"},
            {"san_leader_killing", "OnLeaderKilling"},
            {"san_insert_item", "OnInsertItem"},
            {"san_will_kos", "OnWillKos"},
            {"san_wield_on", "OnWieldOn"},
            {"san_wield_off", "OnWieldOff"},
            {"san_new_sector", "OnNewSector"},
            {"san_remove_item", "OnRemoveItem"},
            {"san_transfer", "OnTransfer"},
            {"san_caught_thief", "OnCaughtThief"},
            {"san_join", "OnJoin"},
            {"san_disband", "OnDisband"},
            {"san_new_map", "OnNewMap"},
            {"san_trap", "OnTrap"},
            {"san_unlock_attempt", "OnUnlockAttempt"},
            {"san_spell_cast", "OnSpellCast"},
            {"san_resurrect", "OnResurrect"},
            {"san_true_seeing", "OnTrueSeeing"}
        };

        private static readonly Dictionary<string, string> SpellEventFunctionNameMapping =
            new Dictionary<string, string>
            {
                {"OnSpellEffect", "OnSpellEffect"},
                {"OnBeginSpellCast", "OnBeginSpellCast"},
                {"OnEndSpellCast", "OnEndSpellCast"},
                {"OnBeginRound", "OnBeginRound"},
                {"OnEndRound", "OnEndRound"},
                {"OnBeginProjectile", "OnBeginProjectile"},
                {"OnEndProjectile", "OnEndProjectile"},
                {"OnBeginRoundD20Ping", "OnBeginRoundD20Ping"},
                {"OnEndRoundD20Ping", "OnEndRoundD20Ping"},
                {"OnAreaOfEffectHit", "OnAreaOfEffectHit"},
                {"OnSpellStruck", "OnSpellStruck"}
            };

        private readonly CompilerOptions _compilerOptions;
        private readonly ScriptEngine _engine;

        private readonly Typings _typings;

        private Dictionary<string, PythonScript> _modules;

        public ScriptConverter(Typings typings)
        {
            _typings = typings;

            _engine = Python.CreateEngine();

            var languageContext = HostingHelpers.GetLanguageContext(_engine);
            _compilerOptions = languageContext.GetCompilerOptions();
        }

        public string ConvertSnippet(string snippet,
            PythonScript withinScript = null,
            Dictionary<string, GuessedType> context = null)
        {
            var ast = ParseSnippet(snippet);

            var script = new PythonScript(withinScript?.Filename ?? "snippet", snippet, ast);

            var expressionConverter = new ExpressionConverter(script, _typings, _modules);
            if (context != null)
            {
                expressionConverter.AddVariables(context);
            }

            ast.Body.Walk(expressionConverter);
            return expressionConverter.Result.ToString();
        }

        internal List<SkillCheck> FindSkillChecks(string snippet)
        {
            var ast = ParseSnippet(snippet);

            var skillCheckFinder = new SkillCheckFinder();
            ast.Walk(skillCheckFinder);
            return skillCheckFinder.Checks;
        }

        private PythonAst ParseSnippet(string snippet)
        {
            var sourceUnit =
                HostingHelpers.GetSourceUnit(
                    _engine.CreateScriptSourceFromString(snippet, SourceCodeKind.SingleStatement));
            var compilerContext = new CompilerContext(sourceUnit, _compilerOptions, ErrorSink.Default);
            var options = new PythonOptions();

            var parser = Parser.CreateParser(compilerContext, options);

            PythonAst ast;
            try
            {
                ast = parser.ParseSingleStatement();
            }
            catch (SyntaxErrorException e)
            {
                throw new ArgumentException($"Failed to parse snippet '{snippet}'.", e);
            }

            return ast;
        }

        internal PythonScript ParseScript(string filename, string content)
        {
            var sourceUnit =
                HostingHelpers.GetSourceUnit(
                    _engine.CreateScriptSourceFromString(content, filename, SourceCodeKind.File));
            var compilerContext = new CompilerContext(sourceUnit, _compilerOptions, ErrorSink.Default);
            var options = new PythonOptions();

            var parser = Parser.CreateParser(compilerContext, options);

            PythonAst ast;
            try
            {
                ast = parser.ParseFile(false);
            }
            catch (SyntaxErrorException e)
            {
                throw new ArgumentException($"Failed to parse file '{filename}' at line {e.Line}.", e);
            }

            return new PythonScript(filename, content, ast);
        }

        private int _currentLine;

        internal string ConvertScript(PythonScript script, Dictionary<string, PythonScript> modules)
        {
            _modules = modules;

            var ast = script.AST;

            if (!(ast.Body is SuiteStatement suiteStatement))
            {
                throw new ArgumentException(
                    $"Expected {script.Filename} to parse into a SuiteStatement, but got: {ast.Body}");
            }

            _currentLine = suiteStatement.Start.Line;

            var declarations = new StringBuilder();
            var declaredFields = new Dictionary<string, GuessedType>();

            HandleTopLevelStatement(script, suiteStatement, declarations, declaredFields);

            return CreateScriptFile(declarations.ToString(), script);
        }

        private bool TryGetComments(PythonScript script, int startLine, int endLine, out string comments)
        {
            var commentLines = script.Content.Split('\n')
                .Skip(startLine)
                .Take(endLine - startLine)
                .Where(l => l.Contains('#'))
                .Select(l => l.Substring(l.IndexOf('#')))
                .Select(l => l.Trim().TrimStart('#').TrimStart())
                .Where(l => l.Length > 0)
                .ToImmutableArray();

            if (commentLines.Length == 0)
            {
                comments = "";
                return false;
            }


            comments = string.Join("\n", commentLines.Select(l => "// " + l)) + "\n";
            return true;
        }

        private void HandleTopLevelStatement(PythonScript script,
            Statement statement,
            StringBuilder declarations,
            Dictionary<string, GuessedType> declaredFields)
        {
            if (statement.Start.Line > _currentLine)
            {
                if (TryGetComments(script, _currentLine, statement.Start.Line, out var comments))
                {
                    declarations.AppendLine(comments);
                }
            }

            _currentLine = statement.End.Line;

            // Top level statements need special treatment
            if (statement is FunctionDefinition functionDefinition)
            {
                HandleFunctionDeclaration(
                    script,
                    declarations,
                    declaredFields,
                    functionDefinition);
            }
            else if (statement is AssignmentStatement assignmentStatement)
            {
                // We have to guess the type of the right hand side to continue
                var rightExpression = assignmentStatement.Right;

                foreach (var leftExpression in assignmentStatement.Left)
                {
                    if (!(leftExpression is NameExpression nameExpression))
                    {
                        continue;
                    }

                    // Assignments to __all__ can be ignored. We could use them to know what is public,
                    // But it applies to a miniscule set of modules anyway
                    if (nameExpression.Name == "__all__")
                    {
                        continue;
                    }

                    string managedFieldType;
                    GuessedType fieldType;
                    var expressionConverter = new ExpressionConverter(script, _typings, _modules);
                    if (rightExpression is DictionaryExpression dictionaryExpression)
                    {
                        expressionConverter.GetDictionaryTypes(dictionaryExpression,
                            out _, out _, out managedFieldType);
                        fieldType = GuessedType.Unknown;
                    }
                    else if (rightExpression is ListExpression listExpression)
                    {
                        expressionConverter.GetListType(listExpression,
                            out var valueType, out managedFieldType);
                        if (valueType == GuessedType.Object)
                        {
                            fieldType = GuessedType.ObjectList;
                        }
                        else
                        {
                            fieldType = GuessedType.UnknownList;
                        }
                    }
                    else
                    {
                        fieldType = expressionConverter.GetExpressionType(rightExpression);
                        managedFieldType = TypeMapping.GuessManagedType(fieldType);
                    }

                    declarations.Append("private static readonly ");
                    declarations.Append(managedFieldType);
                    declarations.Append(' ');
                    declarations.Append(nameExpression.Name);
                    declarations.Append(" = ");
                    declarations.Append(ConvertExpression(script, rightExpression));
                    declarations.AppendLine(";");
                    declaredFields[nameExpression.Name] = fieldType;
                }
            }
            else if (statement is ExpressionStatement expressionStatement)
            {
                if (expressionStatement.Expression is ConstantExpression constantExpression)
                {
                    // Top level expressions of type string are documentation statements
                    var value = constantExpression.Value;
                    if (value is string stringValue)
                    {
                        foreach (var commentLine in stringValue.Split("\n"))
                        {
                            declarations.AppendLine("//" + commentLine);
                        }
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
            else if (statement is FromImportStatement || statement is ImportStatement)
            {
                // Ignore these since we're already taking care of it via the dependencies / modules
            }
            else if (statement is SuiteStatement suiteStatement)
            {
                foreach (var subStatement in suiteStatement.Statements)
                {
                    HandleTopLevelStatement(script, subStatement,
                        declarations, declaredFields);
                }
            }
            else
            {
                declarations.AppendLine("FIXME");
                var exprConverter = new ExpressionConverter(script, _typings, _modules);
                statement.Walk(exprConverter);
                declarations.Append(exprConverter.Result.ToString());
            }
        }

        private void HandleFunctionDeclaration(PythonScript script,
            StringBuilder methodDeclarations,
            Dictionary<string, GuessedType> declaredFields,
            FunctionDefinition functionDefinition)
        {
            // Determine parameter / variable types
            var parameters = new Dictionary<string, GuessedType>();
            bool staticMethod;
            bool overrideFunc;
            string functionName;
            GuessedType? forcedReturnType = null;
            if (script.Type == ScriptType.Object &&
                EventFunctionNameMapping.TryGetValue(functionDefinition.Name, out functionName))
            {
                // All script function paramters are game objects, except for one (san_spell_cast)
                for (var index = 0; index < functionDefinition.Parameters.Count; index++)
                {
                    var parameter = functionDefinition.Parameters[index];
                    if (functionDefinition.Name == "san_spell_cast" && index == 2)
                    {
                        parameters[parameter.Name] = GuessedType.Spell;
                    }
                    else if (functionDefinition.Name == "san_trap" && index == 0)
                    {
                        parameters[parameter.Name] = GuessedType.TrapSprungEvent;
                    }
                    else
                    {
                        parameters[parameter.Name] = GuessedType.Object;
                    }
                }

                forcedReturnType = GuessedType.Bool; // Forced return type
                staticMethod = false;
                overrideFunc = true;
            }
            else if (script.Type == ScriptType.Spell &&
                     SpellEventFunctionNameMapping.TryGetValue(functionDefinition.Name, out functionName))
            {
                // Most script function paramters are spells with some exceptions
                for (var index = 0; index < functionDefinition.Parameters.Count; index++)
                {
                    var parameter = functionDefinition.Parameters[index];
                    parameters[parameter.Name] = GuessedType.Spell;
                    if (functionName == "OnBeginProjectile" || functionName == "OnEndProjectile")
                    {
                        switch (index)
                        {
                            case 1:
                                parameters[parameter.Name] = GuessedType.Object;
                                break;
                            case 2:
                                parameters[parameter.Name] = GuessedType.Integer;
                                break;
                        }
                    }
                }

                forcedReturnType = GuessedType.Void; // Forced return type
                staticMethod = false;
                overrideFunc = true;
            }
            else
            {
                // Do we have an actual mapping for this function???
                functionName = functionDefinition.Name;

                if (_typings.TryGetSignature(script.ClassName,
                    functionName,
                    out var returnType,
                    out var parameterTypes))
                {
                    forcedReturnType = returnType;
                    for (var index = 0; index < functionDefinition.Parameters.Count; index++)
                    {
                        if (index >= parameterTypes.Length)
                        {
                            throw new IndexOutOfRangeException(
                                $"Function {functionName} doesn't have enough parameters in typings file.'");
                        }

                        var parameter = functionDefinition.Parameters[index];
                        parameters[parameter.Name] = parameterTypes[index];
                    }
                }
                else
                {
                    foreach (var parameter in functionDefinition.Parameters)
                    {
                        var parameterName = parameter.Name;
                        // For free-standing functions we have no clue what the argument types could be,
                        // BUT, most of the time they use a consistent naming scheme that'll allow us to guess!
                        parameters[parameterName] = TypeMapping.GuessTypeFromName(parameterName, script.Type);
                    }
                }

                staticMethod = true;
                overrideFunc = false;
            }

            // Now that function parameters are known, convert the function body
            var converter = new ExpressionConverter(script, _typings, _modules);
            converter.ReturnType =
                forcedReturnType; // Set the currently KNOWN return type, which helps in auto-converting return statements
            converter.AddVariables(declaredFields);
            converter.AddVariables(parameters);
            converter.ConvertFunction(functionDefinition);

            // This will allow us to (hopefully) determine the return type
            if (forcedReturnType == null)
            {
                forcedReturnType = converter.ReturnType;
            }

            if (staticMethod)
            {
                script.ExportedFunctions[functionName] = new ExportedFunction
                {
                    PythonName = functionName,
                    CSharpName = functionName,
                    ReturnType = converter.ReturnType.GetValueOrDefault(GuessedType.Unknown)
                };
            }

            methodDeclarations.Append("public");
            if (staticMethod)
            {
                methodDeclarations.Append(" static");
            }

            if (overrideFunc)
            {
                methodDeclarations.Append(" override");
            }

            methodDeclarations.Append(' ');
            methodDeclarations.Append(TypeMapping.GuessManagedType(forcedReturnType));
            methodDeclarations.Append(' ');
            methodDeclarations.Append(functionName);
            methodDeclarations.Append("(");
            for (var i = 0; i < functionDefinition.Parameters.Count; i++)
            {
                if (i > 0)
                {
                    methodDeclarations.Append(", ");
                }

                // All script function paramters are game objects
                var parameter = functionDefinition.Parameters[i];
                methodDeclarations.Append(
                    TypeMapping.GuessManagedType(parameters[parameter.Name]));
                methodDeclarations.Append(' ');
                methodDeclarations.Append(parameter.Name);
                if (parameter.DefaultValue is ConstantExpression defaultValue)
                {
                    methodDeclarations.Append(" = ");
                    methodDeclarations.Append(defaultValue.Value);
                }
            }

            methodDeclarations.AppendLine(") {");
            methodDeclarations.Append(converter.Result.ToString());
            methodDeclarations.AppendLine("}");
        }

        private string CreateScriptFile(string declarations, PythonScript script)
        {
            var annotations = "";
            var extends = "";
            if (script.Type == ScriptType.Object)
            {
                annotations = "[ObjectScript(" + script.ScriptId + ")]";
                extends = " : BaseObjectScript";
            }
            else if (script.Type == ScriptType.Spell)
            {
                annotations = "[SpellScript(" + script.SpellId + ")]";
                extends = " : BaseSpellScript";
            }

            return $@"
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace {script.Namespace} {{
    {annotations}
    public class {script.ClassName}{extends} {{
        {declarations}
    }}
}}
";
        }

        /// <summary>
        ///     This is intended for simple expressions with not alot of recursive logic or arithmetic.
        /// </summary>
        private string ConvertExpression(PythonScript script, Expression expression)
        {
            var expressionConverter = new ExpressionConverter(script, _typings, _modules);
            expression.Walk(expressionConverter);
            return expressionConverter.Result.ToString();
        }
    }

    internal class ExportedFunction
    {
        public string PythonName { get; set; }

        public string CSharpName { get; set; }

        public GuessedType ReturnType { get; set; }
    }
}