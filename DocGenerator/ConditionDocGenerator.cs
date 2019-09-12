using System.Collections.Generic;
using System.IO;
using System.Text;
using SpicyTemple.Core.Systems.D20.Conditions;

namespace DocGenerator
{
    public class ConditionDocGenerator
    {
        private readonly Dictionary<string, ConditionSpecSource> _conditionSpecs;

        public ConditionDocGenerator(Dictionary<string, ConditionSpecSource> conditionSpecs)
        {
            _conditionSpecs = conditionSpecs;
        }

        public void Generate(IEnumerable<ConditionSpec> conditions, string path)
        {
            var parentDir = Path.GetDirectoryName(path);
            if (parentDir != null && !Directory.Exists(path))
            {
                Directory.CreateDirectory(parentDir);
            }

            using var writer = new StreamWriter(path, false, Encoding.UTF8);

            foreach (var condition in conditions)
            {
                WriteCondition(writer, condition);
            }
        }

        private void WriteCondition(TextWriter writer, ConditionSpec conditionSpec)
        {
            writer.WriteLine();
            writer.WriteLine($"=== {conditionSpec.condName}");
            writer.WriteLine("");

            // We can print additional info if we were able to successfully extract the data from the C# source
            if (_conditionSpecs.TryGetValue(conditionSpec.condName, out var condSource))
            {
                if (condSource.TempleDllLocation != null)
                {
                    writer.WriteLine("");
                    writer.WriteLine($"*Temple DLL Location:* {condSource.TempleDllLocation}");
                    writer.WriteLine("");
                }

                writer.WriteLine($".{condSource.Location}");
                writer.WriteLine("[source,csharp]");
                writer.WriteLine("----");
                writer.WriteLine(condSource.Definition);
                writer.WriteLine("----");
            }

            if (conditionSpec.numArgs > 0)
            {
                writer.WriteLine(".Arguments");
                writer.WriteLine("|===");
                writer.WriteLine("| Index | Type | Description");
                writer.WriteLine();

                for (var i = 0; i < conditionSpec.numArgs; i++)
                {
                    writer.WriteLine($"| {i}");
                    writer.WriteLine("| Integer");
                    writer.WriteLine("| ...");
                    writer.WriteLine("");
                }

                writer.WriteLine("|===");
            }
            else
            {
                writer.WriteLine("No Arguments");
                writer.WriteLine("");
            }

            writer.WriteLine();
        }
    }
}