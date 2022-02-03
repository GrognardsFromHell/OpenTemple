using System;

namespace OpenTemple.Core.Logging;

public class ConsoleLogger : LoggerBase
{
    public override void Error(ReadOnlySpan<char> message)
    {
        Console.Write("[e] ");
        Console.WriteLine(message.ToString());
    }

    public override void Warn(ReadOnlySpan<char> message)
    {
        Console.Write("[w] ");
        Console.WriteLine(message.ToString());
    }

    public override void Info(ReadOnlySpan<char> message)
    {
        Console.Write("[i] ");
        Console.WriteLine(message.ToString());
    }

    public override void Debug(ReadOnlySpan<char> message)
    {
        Console.Write("[d] ");
        Console.WriteLine(message.ToString());
    }
}