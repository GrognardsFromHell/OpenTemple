using System;
using System.Collections.Generic;
using OpenTemple.Interop;

namespace OpenTemple.Core.Logging
{
    public static class LoggingSystem
    {
        private static readonly List<WeakReference<DelegatingLogger>> Loggers = new();

        private static LoggerBase _realLogger = new ConsoleLogger();

        static LoggingSystem()
        {
            NativeLogger.Sink = (level, message) =>
            {
                var messageStr = "[native] " + message.ToString();

                switch (level)
                {
                    case NativeLogLevel.Error:
                        _realLogger.Error(messageStr);
                        break;
                    case NativeLogLevel.Warn:
                        _realLogger.Warn(messageStr);
                        break;
                    case NativeLogLevel.Info:
                        _realLogger.Info(messageStr);
                        break;
                    case NativeLogLevel.Debug:
                        _realLogger.Debug(messageStr);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            };
        }

        public static ILogger CreateLogger()
        {
            lock (typeof(LoggingSystem))
            {
                var logger = new DelegatingLogger(_realLogger);
                Loggers.Add(new WeakReference<DelegatingLogger>(logger));
                return logger;
            }
        }

        public static void ChangeLogger(LoggerBase logger)
        {
            lock (typeof(LoggingSystem))
            {
                _realLogger = logger;

                foreach (var loggerRef in Loggers)
                {
                    if (loggerRef.TryGetTarget(out var delegatingLogger))
                    {
                        delegatingLogger.Delegate = logger;
                    }
                }
            }
        }
    }
}