using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using OpenTemple.Core.Systems.D20.Conditions;

namespace DocGenerator
{
    public static class DocGenerator
    {
        public static void Main(string[] args)
        {
            var projectDir = args[0];

            var conditionSourceInfos = ExtractConditionSourceInfo(projectDir);

            var conditionGenerator = new ConditionDocGenerator(conditionSourceInfos);
        }

        private static Dictionary<string, ConditionSpecSource> ExtractConditionSourceInfo(string projectDir)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances[0];

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            var solutionPath = Path.Join(projectDir, "Core/Core.csproj");

            using var workspace = MSBuildWorkspace.Create();
            // Print message for WorkspaceFailed event to help diagnosing project load failures.
            workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

            Console.WriteLine($"Loading solution '{solutionPath}'");

            // Attach progress reporter so we print projects as they are loaded.
            var project = workspace.OpenProjectAsync(solutionPath).Result;
            Console.WriteLine($"Finished loading solution '{solutionPath}'");

            var compilation = project.GetCompilationAsync().Result;

            var visitor = new SourceVisitor(projectDir);

            foreach (var sourceTree in compilation.SyntaxTrees)
            {
                if (!sourceTree.FilePath.Contains("Conditions"))
                {
                    continue;
                }

                var semanticModel = compilation.GetSemanticModel(sourceTree);

                var root = sourceTree.GetRoot();
                visitor.SemanticModel = semanticModel;
                visitor.Visit(root);
            }

            return visitor.ConditionSpecs;
        }
    }
}