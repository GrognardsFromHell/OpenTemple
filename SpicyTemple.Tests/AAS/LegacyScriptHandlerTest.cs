using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTemple.Core.AAS;
using Xunit;
using Xunit.Sdk;

namespace OpenTemple.Tests.AAS
{
    public class LegacyScriptHandlerTest
    {
        [Theory]
        [MemberData(nameof(GetTestData))]
        public void CanConvertLegacyAnimEvent(string legacyScript)
        {
            var handler = new LegacyScriptConverter();
            handler.TryConvert(legacyScript, out var newEvent).Should().Be(true);
        }

        // Reads all known animation scripts from a .txt file (one line per script)
        public static IEnumerable<object[]> GetTestData()
        {
            var stream = typeof(LegacyScriptHandlerTest).Assembly.GetManifestResourceStream(
                typeof(LegacyScriptHandlerTest),
                "animscripts.txt"
            );
            if (stream == null)
            {
                throw new InvalidOperationException("Failed to find embedded resource animscripts.txt");
            }

            var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            stream.Close();

            return content.Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => new object[] {l});
        }
    }
}