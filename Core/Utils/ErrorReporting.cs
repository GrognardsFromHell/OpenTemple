using System;
using System.Collections.Generic;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Utils
{
    public static class ErrorReporting
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static List<UnhandledError> Queue {get;} = new List<UnhandledError>();

        public static void ReportException(Exception e)
        {
            Logger.Error("Uncaught Exception: {0}", e);
            Queue.Add(new UnhandledError(e));
        }

        public readonly struct UnhandledError
        {
            public Exception Error { get; }

            public UnhandledError(Exception error)
            {
                Error = error;
            }
        }
    }
}
