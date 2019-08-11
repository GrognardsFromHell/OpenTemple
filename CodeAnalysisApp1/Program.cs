using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace CodeAnalysisApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances[0];

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                var solutionPath = args[0];
                Console.WriteLine($"Loading solution '{solutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                var project = await workspace.OpenProjectAsync(solutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Finished loading solution '{solutionPath}'");

                var compilation = await project.GetCompilationAsync();

                using (var sw = new StreamWriter("D:/diag.txt")) { 
                    foreach (var diag in compilation.GetDiagnostics())
                    {
                        if (diag.Severity != DiagnosticSeverity.Error)
                        {
                            continue;
                        }
                        sw.WriteLine(diag.ToString());
                    }
                }

                foreach (var sourceTree in compilation.SyntaxTrees)
                {
                    if (!sourceTree.FilePath.Contains(@"\Generated\"))
                    {
                        continue;
                    }

                    Console.WriteLine($"Processing {sourceTree.FilePath}");
                    var semanticModel = compilation.GetSemanticModel(sourceTree);
                    
                    var root = sourceTree.GetRoot();
                    
                    var intToBool = new SemanticRewriter(semanticModel);
                    SyntaxNode newSource = intToBool.Visit(root);
                    Console.WriteLine("Int->Bool: " + intToBool.Count);
                    Console.WriteLine("Void Call Unrolled: " + intToBool.VoidCallUnroll);

                    var doubleToSingle = new DoubleToSingleRewriter();
                    newSource = doubleToSingle.Visit(newSource);
                    Console.WriteLine("Double->Single: " + doubleToSingle.Count);

                    if (newSource != root)
                    {
                        Console.WriteLine("Rewriting " + sourceTree.FilePath);
                        File.WriteAllText(sourceTree.FilePath, newSource.ToFullString());
                    }
                }

                // Get all analyzers
                var analyzers = ImmutableArray.CreateBuilder<CodeRefactoringProvider>();
                Assembly.GetAssembly(typeof(CodeRefactoringProvider))
                        .GetTypes()
                        .Where(x => typeof(CodeRefactoringProvider).IsAssignableFrom(x))
                        .Where(x => !x.IsAbstract)
                        .Select(Activator.CreateInstance)
                        .Cast<CodeRefactoringProvider>()
                        .ToList()
                        .ForEach(x => analyzers.Add(x));
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
        
    }


    public static class SyntaxTreeExtensions
    {
        public static void Dump(this SyntaxTree tree)
        {
            var writer = new ConsoleDumpWalker();
            writer.Visit(tree.GetRoot());
        }

        class ConsoleDumpWalker : SyntaxWalker
        {
            public override void Visit(SyntaxNode node)
            {
                int padding = node.Ancestors().Count();
                //To identify leaf nodes vs nodes with children
                string prepend = node.ChildNodes().Any() ? "[-]" : "[.]";
                //Get the type of the node
                string line = new String(' ', padding) + prepend +
                                        " " + node.GetType().ToString();
                //Write the line
                System.Console.WriteLine(line);
                base.Visit(node);
            }

        }
    }

}
