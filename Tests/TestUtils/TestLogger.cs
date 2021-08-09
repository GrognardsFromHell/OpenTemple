using System;
using NUnit.Framework;
using OpenTemple.Core.Logging;

namespace OpenTemple.Tests.TestUtils
{
    public class TestLogger : LoggerBase
    {
        public override void Error(ReadOnlySpan<char> message)
        {
            TestContext.Error.Write("[e] ");
            TestContext.Error.WriteLine(message.ToString());
        }

        public override void Warn(ReadOnlySpan<char> message)
        {
            TestContext.Error.Write("[e] ");
            TestContext.Error.WriteLine(message.ToString());
        }

        public override void Info(ReadOnlySpan<char> message)
        {
            TestContext.Out.Write("[i] ");
            TestContext.Out.WriteLine(message.ToString());
        }

        public override void Debug(ReadOnlySpan<char> message)
        {
            TestContext.Out.Write("[d] ");
            TestContext.Out.WriteLine(message.ToString());
        }
    }
}