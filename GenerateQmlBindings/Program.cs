using System;
using System.IO;
using QmlBuildTools;

namespace GenerateQmlBindings
{
    class Program
    {
        static void Main(string[] args)
        {
            using var qmlInfo = new QmlInfo(@"D:\OpenTemple\Data\ui");
            foreach (var path in Directory.EnumerateFiles(@"D:\OpenTemple\Data\ui",
                "*.qml",
                SearchOption.AllDirectories))
            {
                Console.WriteLine($"Loading {path}");
                qmlInfo.LoadFile(path);
            }

            var code = qmlInfo.Process();
            File.WriteAllText(@"D:\OpenTemple\Core\Qml.Generated.cs", code);
        }
    }
}