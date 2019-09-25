using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IronPython.Modules;
using IronPython.Runtime.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using SpicyTemple.Core.IO.TroikaArchives;
using SpicyTemple.Core.TigSubsystems;

namespace ScriptConversion
{
    public class ScriptConversion
    {
        private readonly string _outputDirectory;

        private readonly Typings _typings;

        private readonly ScriptConverter converter;

        // Format the script code before writing it to a file
        private AdhocWorkspace workspace;

        private Project project;

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
                Console.WriteLine("Scripts with clashing names: " + string.Join(", ", grouping.Select(c => c.Filename)));
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

                File.Delete(outputFile);
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

            var syntaxTree = CSharpSyntaxTree.ParseText(document.GetTextAsync().Result);
            var formattedNode = Formatter.Format(syntaxTree.GetRoot(), workspace);

            File.WriteAllText(outputFile, formattedNode.ToFullString());
        }
    }
}