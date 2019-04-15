using System;
using JetBrains.Annotations;

namespace SpicyTemple.Core.Logging
{
    public interface ILogger
    {
        void Error(string message);

        [StringFormatMethod("format")]
        void Error<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Error<T1, T2>(string format, T1 arg1, T2 arg2);

        void Info(string message);

        [StringFormatMethod("format")]
        void Info<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Info<T1, T2>(string format, T1 arg1, T2 arg2);

        void Warn(string message);

        [StringFormatMethod("format")]
        void Warn<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Warn<T1, T2>(string format, T1 arg1, T2 arg2);
    }

    public abstract class LoggerBase : ILogger
    {
        public abstract void Error(string message);

        public abstract void Warn(string message);

        public abstract void Info(string message);

        public void Error<T1>(string format, T1 arg1)
        {
            Error(string.Format(format, arg1));
        }

        public void Error<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Error(string.Format(format, arg1, arg2));
        }

        public void Warn<T1>(string format, T1 arg1)
        {
            Warn(string.Format(format, arg1));
        }

        public void Warn<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Warn(string.Format(format, arg1, arg2));
        }

        public void Info<T1>(string format, T1 arg1)
        {
            Info(string.Format(format, arg1));
        }

        public void Info<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Info(string.Format(format, arg1, arg2));
        }
    }

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
    }
}