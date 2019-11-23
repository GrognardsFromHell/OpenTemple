using System.IO;
using System.Reflection;

namespace SpicyTemple.Tests
{
    public static class TestData
    {
        public static string SolutionDir { get; }

        public static string ProjectDir { get; }

        public static string GetPath(string relativePath)
        {
            return Path.Join(ProjectDir, relativePath);
        }

        static TestData()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(TestData)).Location);
            var solutionRoot = assemblyDir;
            while (!File.Exists(Path.Join(solutionRoot, "SpicyTemple.sln")))
            {
                solutionRoot = Path.GetDirectoryName(solutionRoot);
                if (solutionRoot == null)
                {
                    throw new FileNotFoundException("Failed to find project root directory, starting at " +
                                                    assemblyDir);
                }
            }

            SolutionDir = solutionRoot;
            ProjectDir = Path.Join(solutionRoot, "SpicyTemple.Tests");
        }
    }
}