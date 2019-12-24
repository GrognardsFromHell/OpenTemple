using System;
using System.Collections.Generic;

namespace OpenTemple.Core.Logging
{
    public static class LoggingSystem
    {
        private static LoggerBase _realLogger = new ConsoleLogger();

        private static List<WeakReference<DelegatingLogger>> _loggers = new List<WeakReference<DelegatingLogger>>();

        public static ILogger CreateLogger()
        {
            lock (typeof(LoggingSystem))
            {
                var logger = new DelegatingLogger(_realLogger);
                _loggers.Add(new WeakReference<DelegatingLogger>(logger));
                return logger;
            }
        }

        public static void ChangeLogger(LoggerBase logger)
        {
            lock (typeof(LoggingSystem))
            {
                _realLogger = logger;

                foreach (var loggerRef in _loggers)
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