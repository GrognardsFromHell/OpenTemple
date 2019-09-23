using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using IronPython.Compiler.Ast;
using SpicyTemple.Core.Systems.Pathfinding;
using Path = System.IO.Path;

namespace ScriptConversion
{
    internal class PythonScript
    {
        private static readonly Regex PythonScriptNamePattern = new Regex(@"^py(\d+)(.*)\.py$");

        private static readonly Regex SpellNamePattern = new Regex(@"^Spell(\d+)[\s\-]*(.*)\.py$");

        public string Filename { get; }

        public string Content { get; }

        public PythonAst AST { get; }

        public ISet<string> ImportedModules { get; }

        public ScriptType Type { get; private set; }

        public int ScriptId { get; private set; }

        public int SpellId { get; private set; }

        public string ModuleName => Path.ChangeExtension(Filename, null);

        public string Namespace { get; private set; }

        public string ClassName { get; private set; }

        public string OutputPath { get; private set; }

        public Dictionary<string, ExportedFunction> ExportedFunctions { get; set; }
            = new Dictionary<string, ExportedFunction>();

        public PythonScript(string filename, string content, PythonAst ast)
        {
            Filename = filename;
            Namespace = "Scripts";
            ClassName = CreateClassName(Path.ChangeExtension(filename, null));
            Content = content;
            AST = ast;

            Type = ScriptType.Module;
            ScriptId = -1;
            OutputPath = ClassName + ".cs";

            // Check if the filename matches the pattern for "attached" scripts that will contain san_ events.
            var match = PythonScriptNamePattern.Match(filename);
            if (match.Success)
            {
                Type = ScriptType.Object;
                ScriptId = int.Parse(match.Groups[1].Value);
                ClassName = CreateClassName(match.Groups[2].Value);
                OutputPath = ClassName + ".cs";
                Namespace = "Scripts";
            }
            else
            {
                match = SpellNamePattern.Match(filename);
                if (match.Success)
                {
                    Type = ScriptType.Spell;
                    SpellId = int.Parse(match.Groups[1].Value);
                    ClassName = CreateClassName(match.Groups[2].Value);
                    OutputPath = "Spells/" + ClassName + ".cs";
                    Namespace = "Scripts.Spells";
                }
            }

            // Find imported modules
            var importedWalker = new ImportedModulesWalker();
            ast.Walk(importedWalker);
            ImportedModules = importedWalker.ImportedModules;
            ImportedModules.Remove("toee");
            ImportedModules.Remove("__main__");
            ImportedModules.Remove("math");
            ImportedModules.Remove("sys");
            ImportedModules.Remove("array");
            ImportedModules.Remove("t"); // because it has a circular dependency
            ImportedModules.Remove("co8Util");
            ImportedModules.Remove("co8Util.Logger");
            ImportedModules.Remove("co8Util.PersistentData");
            ImportedModules.Remove("co8Util.TimedEvent");
            ImportedModules.Remove("co8Util.ObjHandling");
            ImportedModules.Remove("co8Util.Enum");
            ImportedModules.Remove("_include"); // Co8 hack
            if (ModuleName != "utilities")
            {
                ImportedModules.Add("utilities"); // Implicitly imported by C++ code into __main__
            }
        }

        private static string CreateClassName(string name)
        {
            var startOfWord = true;
            var builder = new StringBuilder(name.Length);

            foreach (var ch in name)
            {
                if (!char.IsLetterOrDigit(ch))
                {
                    startOfWord = true;
                    continue;
                }

                if (startOfWord)
                {
                    builder.Append(char.ToUpperInvariant(ch));
                    startOfWord = false;
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }
    }

    public enum ScriptType
    {
        /// <summary>
        /// Loose py file with no special purpose.
        /// </summary>
        Module,

        /// <summary>
        /// Attachable script.
        /// </summary>
        Object,

        Spell
    }
}