using System;

namespace OpenTemple.Core.Logging
{
    public class ColoredConsoleLogger : ConsoleLogger
    {
        public override void Error(ReadOnlySpan<char> message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            base.Error(message);
            Console.ResetColor();
        }

        public override void Warn(ReadOnlySpan<char> message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            base.Warn(message);
            Console.ResetColor();
        }

        public override void Debug(ReadOnlySpan<char> message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            base.Debug(message);
            Console.ResetColor();
        }
    }
}