using System;

namespace OpenTemple.Core.Logging
{
    public abstract class LoggerBase : ILogger
    {
        public abstract void Error(ReadOnlySpan<char> message);

        public abstract void Warn(ReadOnlySpan<char> message);

        public abstract void Info(ReadOnlySpan<char> message);

        public abstract void Debug(ReadOnlySpan<char> message);

        public void Error<T1>(string format, T1 arg1)
        {
            Error(string.Format(format, arg1).AsSpan());
        }

        public void Error<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Error(string.Format(format, arg1, arg2).AsSpan());
        }

        public void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Error(string.Format(format, arg1, arg2, arg3).AsSpan());
        }

        public void Warn<T1>(string format, T1 arg1)
        {
            Warn(string.Format(format, arg1).AsSpan());
        }

        public void Warn<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Warn(string.Format(format, arg1, arg2).AsSpan());
        }

        public void Warn<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Warn(string.Format(format, arg1, arg2, arg3).AsSpan());
        }

        public void Warn(string format, params object[] args)
        {
            Warn(string.Format(format, args).AsSpan());
        }

        public void Info<T1>(string format, T1 arg1)
        {
            Info(string.Format(format, arg1).AsSpan());
        }

        public void Info<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Info(string.Format(format, arg1, arg2).AsSpan());
        }

        public void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Info(string.Format(format, arg1, arg2, arg3).AsSpan());
        }

        public void Info(string format, params object[] args)
        {
            Info(string.Format(format, args).AsSpan());
        }

        public void Debug<T1>(string format, T1 arg1)
        {
            Debug(string.Format(format, arg1).AsSpan());
        }

        public void Debug<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Debug(string.Format(format, arg1, arg2).AsSpan());
        }

        public void Debug<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Debug(string.Format(format, arg1, arg2, arg3).AsSpan());
        }

        public void Debug(string format, params object[] args)
        {
            Debug(string.Format(format, args).AsSpan());
        }
    }
}