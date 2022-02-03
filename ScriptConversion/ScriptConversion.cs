using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.TigSubsystems;

namespace ScriptConversion;

public class ScriptConversion
{
    private readonly string _outputDirectory;

    private readonly Typings _typings;

    private readonly ScriptConverter converter;

    private readonly List<PythonScript> _convertedScripts = new List<PythonScript>();

    // Format the script code before writing it to a file
    private AdhocWorkspace workspace;

    private Project project;

    private bool _writeScripts = true;

    private bool _writeDialog = false;

    private static readonly Dictionary<string, GuessedType> DialogContext =
        new Dictionary<string, GuessedType>
        {
            {"picked_line", GuessedType.Integer},
            {"pc", GuessedType.Object},
            {"npc", GuessedType.Object}
        };

    public ScriptConversion(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        workspace = new AdhocWorkspace();
        project = workspace.AddProject(ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Default, "Scripts",
            "Scripts", LanguageNames.CSharp));
        _typings = new Typings();
        converter = new ScriptConverter(_typings);
    }

    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: scriptconversion <scripts|file> ...");
            return;
        }

        string mode = args[0];
        if (mode == "scripts")
        {
            ConvertScripts(args.Skip(1).ToArray());
            return;
        }
        else if (mode == "folder")
        {
            Tig.FS = TroikaVfs.CreateFromInstallationDir(args[1]);

            Directory.CreateDirectory("temp");
            var conversion = new ScriptConversion("temp");
            foreach (var pythonFile in Directory.EnumerateFiles(args[2], "*.py"))
            {
                conversion.ConvertSingleFile(pythonFile, ScriptType.TemplePlusCondition);
            }
        }
        else
        {
            Console.WriteLine("Unknown mode: " + mode);
        }
    }

    private void ConvertSingleFile(string path, ScriptType scriptType)
    {
        var scriptContent = File.ReadAllText(path);

        PythonScript script;
        try
        {
            script = converter.ParseScript(Path.GetFileName(path), scriptContent);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to parse {path}: {e.Message}");
            return;
        }

        script.Type = scriptType;
        ConvertScript(script, new Dictionary<string, PythonScript>());
    }

    private static void ConvertScripts(string[] args)
    {
        var outputDir = "scripts";
        if (args.Length != 1 && args.Length != 2)
        {
            Console.WriteLine("Usage: scriptconversion <toee-dir> [<script-out-dir>]");
            return;
        }

        var installationDir = args[0];
        if (args.Length > 1)
        {
            outputDir = args[1];
        }

        Directory.CreateDirectory(outputDir);

        Tig.FS = TroikaVfs.CreateFromInstallationDir(installationDir);

        var conversion = new ScriptConversion(outputDir);
        conversion.LoadTyping(outputDir);
        conversion.ConvertScripts();
        conversion.ConvertDialog();
    }

    private void ConvertDialog()
    {
        var dialogFiles = Tig.FS.ListDirectory("dlg")
            .Where(f => f.EndsWith(".dlg"))
            .Distinct()
            .ToList();

        Console.WriteLine($"Found {dialogFiles.Count} dialog files.");

        var dlgFilePattern = new Regex(@"(\d{5}).*");

        foreach (var dialogFile in dialogFiles)
        {
            var m = dlgFilePattern.Match(dialogFile);
            if (!m.Success)
            {
                Console.WriteLine($"Skipping dialog file that doesn't match expected pattern: {dialogFile}'.");
                continue;
            }

            var scriptId = int.Parse(m.Groups[1].Value);
            var associatedScript = _convertedScripts.FirstOrDefault(s => s.Type == ScriptType.Object
                                                                         && s.ScriptId == scriptId);
            if (associatedScript == null)
            {
                Console.WriteLine($"Dialog file {dialogFile} with id {scriptId} has no associated script!");
                continue;
            }

            var outputDir = Path.Join(
                Path.GetDirectoryName(associatedScript.OutputPath),
                "Dialog"
            );
            Directory.CreateDirectory(Path.Combine(_outputDirectory, outputDir));

            var outputPath = Path.Join(outputDir,
                Path.GetFileNameWithoutExtension(associatedScript.OutputPath) + "Dialog.cs");
            if (_writeDialog)
            {
                File.Delete(Path.Combine(_outputDirectory, outputPath));
            }

            var dialogContent = Tig.FS.ReadTextFile("dlg/" + dialogFile);
            var parser = new DialogScriptParser(dialogFile, dialogContent);

            // We're going to build a class file for the dialog script that contains two methods,
            // and extends from the object script of the same script id

            var conditions = new List<(int, string, string)>();
            var effects = new List<(int, string, string)>();
            var skillChecks = new List<(int, SkillCheck)>();

            while (parser.GetSingleLine(out var dialogLine, out var fileLine))
            {
                var effectPython = dialogLine.effectField;
                if (effectPython == "pc.barter(npc)" &&
                    dialogLine.txt.StartsWith("b:", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip bogus barter effect (this is implied by B:)
                }

                try
                {
                    if (!string.IsNullOrEmpty(effectPython))
                    {
                        var converted = converter.ConvertSnippet(effectPython, associatedScript, DialogContext);
                        effects.Add((dialogLine.key, effectPython, converted));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"[e] Failed to convert effect for line {dialogLine.key} ({effectPython}): {e}");
                    effects.Add((dialogLine.key, effectPython, null));
                }

                if (dialogLine.IsPcLine)
                {
                    var conditionPython = dialogLine.testField;
                    try
                    {
                        if (!string.IsNullOrEmpty(conditionPython))
                        {
                            foreach (var skillCheck in converter.FindSkillChecks(conditionPython))
                            {
                                skillChecks.Add((dialogLine.key, skillCheck));
                            }

                            var converted =
                                converter.ConvertSnippet(conditionPython, associatedScript, DialogContext);
                            conditions.Add((dialogLine.key, conditionPython, converted));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            $"[e] Failed to convert condition for line {dialogLine.key} ({conditionPython}): {e}");
                        conditions.Add((dialogLine.key, conditionPython, null));
                    }
                }
            }

            if (_writeDialog)
            {
                var dialogScript = new StringBuilder();

                dialogScript.AppendLine(@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;
");
                dialogScript.AppendLine("namespace " + associatedScript.Namespace + ".Dialog");
                dialogScript.AppendLine("{");
                dialogScript.Append("[DialogScript(").Append(associatedScript.ScriptId).AppendLine(")]");
                dialogScript.AppendLine("public class " + associatedScript.ClassName + "Dialog : " +
                                        associatedScript.ClassName + ", IDialogScript");
                dialogScript.AppendLine("{");

                WriteDialogMethod(dialogScript, conditions, false);
                WriteDialogMethod(dialogScript, effects, true);
                WriteSkillChecksMethod(dialogScript, skillChecks);

                dialogScript.AppendLine("}");
                dialogScript.AppendLine("}");

                var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
                var syntaxTree = CSharpSyntaxTree.ParseText(dialogScript.ToString(), parseOptions);
                var formattedNode = Formatter.Format(syntaxTree.GetRoot(), workspace);

                File.WriteAllText(Path.Join(_outputDirectory, outputPath), formattedNode.ToFullString());
            }
        }
    }

    private static void WriteDialogMethod(StringBuilder dialogScript, List<(int, string, string)> conditions,
        bool effects)
    {
        if (effects)
        {
            dialogScript.AppendLine(
                "public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)");
        }
        else
        {
            dialogScript.AppendLine(
                "public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)");
        }

        dialogScript.AppendLine("{");
        dialogScript.AppendLine("switch (lineNumber)");
        dialogScript.AppendLine("{");
        for (var i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].Item1 == -1)
            {
                continue; // Already printed elsewhere
            }

            dialogScript.Append("case ").Append(conditions[i].Item1).AppendLine(":");
            // Collapse identical conditions
            for (var j = i + 1; j < conditions.Count; j++)
            {
                if (conditions[j].Item2 == conditions[i].Item2)
                {
                    dialogScript.Append("case ").Append(conditions[j].Item1).AppendLine(":");
                    conditions[j] = (-1, null, null);
                }
            }

            var escapedOriginal = SyntaxFactory
                .LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(conditions[i].Item2))
                .ToFullString();
            dialogScript.Append("originalScript = ").Append(escapedOriginal).AppendLine(";");
            var converted = conditions[i].Item3;
            if (converted != null)
            {
                if (!effects)
                {
                    dialogScript.Append("return ");
                }

                dialogScript.Append(converted);
                if (converted.EndsWith(";"))
                {
                    dialogScript.AppendLine();
                }
                else
                {
                    dialogScript.AppendLine(";");
                }

                if (effects)
                {
                    dialogScript.AppendLine("break;");
                }
            }
            else
            {
                dialogScript.AppendLine("throw new NotSupportedException(\"Conversion failed.\");");
            }
        }

        dialogScript.AppendLine("default:");
        dialogScript.AppendLine("originalScript = null;");
        if (effects)
        {
            dialogScript.AppendLine("return;");
        }
        else
        {
            dialogScript.AppendLine("return true;");
        }

        dialogScript.AppendLine("}");
        dialogScript.AppendLine("}");
    }

    private void WriteSkillChecksMethod(StringBuilder dialogScript, List<(int, SkillCheck)> skillChecks)
    {
        dialogScript.AppendLine("public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)");
        dialogScript.AppendLine("{");
        dialogScript.AppendLine("switch (lineNumber)");
        dialogScript.AppendLine("{");

        for (var i = 0; i < skillChecks.Count; i++)
        {
            var (lineNumber, skillCheck) = skillChecks[i];
            if (lineNumber == -1)
            {
                continue; // already handled elsewhere
            }

            dialogScript.Append("case ").Append(lineNumber).AppendLine(":");
            // Collapse identical skill checks
            for (var j = i + 1; j < skillChecks.Count; j++)
            {
                if (skillCheck.Skill == skillChecks[j].Item2.Skill
                    && skillCheck.Ranks == skillChecks[j].Item2.Ranks)
                {
                    dialogScript.Append("case ").Append(skillChecks[j].Item1).AppendLine(":");
                    skillChecks[j] = (-1, default);
                }
            }

            dialogScript.Append("skillChecks = new DialogSkillChecks(");
            dialogScript.Append("SkillId.");
            dialogScript.Append(skillCheck.Skill.ToString());
            dialogScript.Append(", ");
            dialogScript.Append(skillCheck.Ranks);
            dialogScript.AppendLine(");");
            dialogScript.AppendLine("return true;");
        }

        dialogScript.AppendLine("default:");
        dialogScript.AppendLine("skillChecks = default;");
        dialogScript.AppendLine("return false;");
        dialogScript.AppendLine("}");
        dialogScript.AppendLine("}");
    }

    private void LoadTyping(string dir)
    {
        var typingFile = Path.Join(dir, "typing.csv");
        if (File.Exists(typingFile))
        {
            _typings.Load(typingFile);
        }
    }

    private bool HasUnmetDependencies(PythonScript script, IDictionary<string, PythonScript> availableModules)
    {
        var dependencies = script.ImportedModules;
        foreach (var dependency in dependencies)
        {
            if (!availableModules.ContainsKey(dependency))
            {
                return true;
            }
        }

        return false;
    }

    private void ConvertScripts()
    {
        var pythonScripts = Tig.FS.ListDirectory("scr")
            .Where(f => f != "__future__.py")
            .Where(f => f != "copy_reg.py")
            .Where(f => f != "types.py")
            .Where(f => f.EndsWith(".py"))
            .Distinct()
            .ToList();

        // Load scripts first
        var scripts = new List<PythonScript>();

        foreach (var pythonScript in pythonScripts)
        {
            var script = ParseScript(pythonScript);
            if (script != null)
            {
                scripts.Add(script);
            }
        }

        // Search for duplicates
        foreach (var grouping in scripts.GroupBy(s => s.ClassName))
        {
            var count = grouping.Count();
            if (count == 1)
            {
                continue;
            }

            var suffix = 1;
            Console.WriteLine("Scripts with clashing names: " +
                              string.Join(", ", grouping.Select(c => c.Filename)));
            foreach (var script in grouping)
            {
                script.ClassName += suffix;
                script.OutputPath = Path.ChangeExtension(script.OutputPath, null) + suffix + ".cs";
                suffix++;
            }
        }

        foreach (var script in scripts)
        {
            var outputFile = Path.Join(_outputDirectory, script.OutputPath);
            var parentDir = Path.GetDirectoryName(outputFile);
            if (parentDir != null)
            {
                Directory.CreateDirectory(parentDir);
            }

            if (_writeScripts)
            {
                File.Delete(outputFile);
            }
        }

        Console.WriteLine($"Parsed {scripts.Count} scripts...");

        // Convert all scripts without imports first
        int lastIteration = -1;
        var remaining = new List<PythonScript>(scripts);
        var modules = new Dictionary<string, PythonScript>();
        while (lastIteration != remaining.Count)
        {
            lastIteration = remaining.Count;

            for (var i = 0; i < remaining.Count; i++)
            {
                var script = remaining[i];
                if (!HasUnmetDependencies(script, modules))
                {
                    _convertedScripts.Add(script);

                    try
                    {
                        ConvertScript(script, modules);

                        modules[script.ModuleName] = script;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to convert {script.Filename}: {e.Message}");
                        throw;
                    }

                    remaining.RemoveAt(i);
                    i--;
                }
            }
        }

        if (remaining.Count > 0)
        {
            Console.WriteLine($"Failed to convert {remaining.Count} scripts due to missing dependencies:");
            foreach (var pythonScript in remaining)
            {
                if (pythonScript.ModuleName == "utilities")
                {
                    Debugger.Break();
                }

                Console.WriteLine("  - " + pythonScript.Filename);
                foreach (var dependency in pythonScript.ImportedModules)
                {
                    Console.Write("    - " + dependency);
                    if (!modules.ContainsKey(dependency))
                    {
                        Console.Write(" [missing]");
                    }

                    Console.WriteLine();
                }
            }
        }
    }

    private PythonScript ParseScript(string pythonScript)
    {
        var content = Tig.FS.ReadTextFile("scr/" + pythonScript);

        try
        {
            return converter.ParseScript(pythonScript, content);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to parse {pythonScript}: {e.Message}");
            return null;
        }
    }

    private void ConvertScript(PythonScript script, Dictionary<string, PythonScript> modules)
    {
        var outputFile = Path.Join(_outputDirectory, script.OutputPath);

        var convertedSource = converter.ConvertScript(script, modules);

        var document = workspace.AddDocument(project.Id, script.OutputPath, SourceText.From(convertedSource));

        if (_writeScripts)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(document.GetTextAsync().Result);
            var formattedNode = Formatter.Format(syntaxTree.GetRoot(), workspace);

            File.WriteAllText(outputFile, formattedNode.ToFullString());
        }
    }
}