using System;
using NUnit.Framework;
using OpenTemple.Core.Logging;

namespace OpenTemple.Tests.TestUtils;

public class TestLogger : LoggerBase
{
    public override void Error(ReadOnlySpan<char> message)
    {
        TestContext.Out.WriteLine("[e] " + message.ToString());
    }

    public override void Warn(ReadOnlySpan<char> message)
    {
        TestContext.Out.WriteLine("[w] " + message.ToString());
    }

    public override void Info(ReadOnlySpan<char> message)
    {
        TestContext.Out.WriteLine("[i] " + message.ToString());
    }

    public override void Debug(ReadOnlySpan<char> message)
    {
        TestContext.Out.WriteLine("[d] " + message.ToString());
    }
}