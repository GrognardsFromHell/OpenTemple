using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace SpicyTemple.Tests
{
    /// <summary>
    /// See https://www.patriksvensson.se/2017/11/using-embedded-resources-in-xunit-tests
    /// </summary>
    public sealed class EmbeddedResourceDataAttribute : DataAttribute
    {
        private readonly string[] _args;

        public EmbeddedResourceDataAttribute(params string[] args)
        {
            _args = args;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var testType = testMethod.DeclaringType;

            var result = new object[_args.Length];
            for (var index = 0; index < _args.Length; index++)
            {
                result[index] = ReadManifestData(testType, _args[index]);
            }

            return new[] {result};
        }

        public static string ReadManifestData(Type containingType, string resourceName)
        {
            var assembly = containingType.GetTypeInfo().Assembly;
            if (!resourceName.StartsWith("/"))
            {
                // Assumed to be relative to the class containing the method
                resourceName = containingType.Namespace + "." + resourceName;
            }
            else
            {
                resourceName = resourceName.Substring(1); // Strip the leading slash
            }
            resourceName = resourceName.Replace("/", ".");
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    var existingNames = assembly.GetManifestResourceNames();

                    throw new InvalidOperationException("Could not load manifest resource stream.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}