using System;

namespace OpenTemple.Core.Logging
{
    public class ConsoleLogger : LoggerBase
    {
        public override void Error(string message)
        {
            Console.Write("[e] ");
            Console.WriteLine(message);
        }

        public override void Warn(string message)
        {
            Console.Write("[w] ");
            Console.WriteLine(message);
        }

        public override void Info(string message)
        {
            Console.Write("[i] ");
            Console.WriteLine(message);
        }

        public override void Debug(string message)
        {
            Console.Write("[d] ");
            Console.WriteLine(message);
        }
    }
}