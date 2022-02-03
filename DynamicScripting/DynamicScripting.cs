using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CSharp.RuntimeBinder;
using OpenTemple.Core;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Scripting;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using Document = Microsoft.CodeAnalysis.Document;

namespace OpenTemple.DynamicScripting
{
    /// <summary>
    /// The REPL subsystem used by the console.
    /// </summary>
    public class DynamicScripting : IDynamicScripting
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static readonly string[] Usings =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            typeof(UiSystems).Namespace,
            typeof(GameSystems).Namespace,
            typeof(GameObject).Namespace,
            typeof(locXY).Namespace,
            typeof(Vector2).Namespace,
            typeof(SkillId).Namespace,
            typeof(Dice).Namespace
        };

        private ScriptOptions _scriptOptions = ScriptOptions.Default
            .AddReferences(
                // Allow access to anything from the Core assembly
                typeof(ReplGlobals).Assembly,
                typeof(ExpandoObject).Assembly,
                typeof(CSharpArgumentInfo).Assembly, // Microsoft.CSharp
                typeof(Enumerable).Assembly, // Linq
                typeof(Dictionary<,>).Assembly // Generic Collections
            )
            .WithImports(Usings);

        private readonly CSharpCompilationOptions _compilationOptions = new(
            OutputKind.DynamicallyLinkedLibrary,
            usings: Usings
        );

        private readonly ReplGlobals _globals = new();

        private Document _scriptDocument;

        public DynamicScripting()
        {
            // The default will try to load VisualBasic, which we explicitly want to exclude
            var mefServices = MefHostServices.Create(new []
            {
                Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
            });
            var workspace = new AdhocWorkspace(mefServices);

            var scriptProjectInfo = ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "Script",
                    "Script",
                    LanguageNames.CSharp,
                    isSubmission: true,
                    hostObjectType: typeof(ReplGlobals))
                .WithParseOptions(new CSharpParseOptions(
                    LanguageVersion.LatestMajor,
                    kind: SourceCodeKind.Script
                ))
                .WithMetadataReferences(new[]
                {
                    MetadataReference.CreateFromFile(typeof(IDynamicScripting).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DynamicScripting).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Object).Assembly.Location)
                })
                .WithCompilationOptions(_compilationOptions);
            var project = workspace.AddProject(scriptProjectInfo);

            var scriptDocumentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id), "Script",
                sourceCodeKind: SourceCodeKind.Script);
            _scriptDocument = workspace.AddDocument(scriptDocumentInfo);
        }

        public object EvaluateExpression(string command)
        {
            try
            {
                return CSharpScript.EvaluateAsync(command, _scriptOptions, _globals).Result;
            }
            catch (Exception e)
            {
                return "[error] " + e.Message;
            }
        }

        public string Complete(string command)
        {
            var completions = GetCompletions(command);
            if (completions.Count > 0)
            {
                var completion = completions[0];
                return command.Substring(0, completion.Span.Start) + completion.DisplayText;
            }

            return command;
        }

        public async Task<object> RunScriptAsync(string path)
        {
            var scriptText = Tig.FS.ReadTextFile(path);
            var script = CSharpScript.Create(scriptText, _scriptOptions, typeof(ReplGlobals));
            try
            {
                Globals.GameLoop.AcquireGlobalLock();
                try
                {
                    var result = await script.RunAsync(_globals);
                    _globals.Print(result.ReturnValue);
                    return result.ReturnValue;
                }
                finally
                {
                    Globals.GameLoop.ReleaseGlobalLock();
                }
            }
            catch (Exception e)
            {
                Tig.Console.Append("[error] " + e.Message);
                Logger.Info("Script failed with exception: {0}", e);
                return e;
            }
        }

        public void RunStartupScripts()
        {
            if (Tig.FS.FileExists("scripts/startup.csx"))
            {
                _ = RunScriptAsync("scripts/startup.csx");
            }
        }

        public void AddAssembly(Assembly assembly)
        {
            _scriptOptions = _scriptOptions.AddReferences(assembly);
        }

        public List<CompletionItem> GetCompletions(string codeSnippet)
        {
            _scriptDocument = _scriptDocument.WithText(SourceText.From(codeSnippet));

            // cursor position is at the end
            var position = codeSnippet.Length;

            var completionService = CompletionService.GetService(_scriptDocument);
            var completions = completionService.GetCompletionsAsync(_scriptDocument, position).Result;
            if (completions == null)
            {
                return new List<CompletionItem>();
            }

            // We now need to filter all possible completions
            // We use something from the RoslynPad project here to do this,
            // and also have to dig into Roslyn internals to not duplicate this code
            var helper = CompletionHelper.Create(_scriptDocument);
            var text = _scriptDocument.GetTextAsync().Result;
            var textSpanToText = new Dictionary<TextSpan, string>();

            bool MatchesFilterText(CompletionItem item)
            {
                var filterText = GetFilterText(item, text, textSpanToText);
                if (string.IsNullOrEmpty(filterText)) return true;
                return helper.MatchesPattern(item.FilterText, filterText, CultureInfo.InvariantCulture);
            }

            return completions.Items
                .Where(item => !item.Tags.Contains("Keyword"))
                .Where(MatchesFilterText)
                .ToList();
        }

        private static string GetFilterText(CompletionItem item, SourceText text,
            Dictionary<TextSpan, string> textSpanToText)
        {
            var textSpan = item.Span;
            if (!textSpanToText.TryGetValue(textSpan, out var filterText))
            {
                filterText = text.GetSubText(textSpan).ToString();
                textSpanToText[textSpan] = filterText;
            }

            return filterText;
        }
    }
}