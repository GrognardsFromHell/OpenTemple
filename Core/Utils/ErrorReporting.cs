using System;
using System.Collections.Generic;

namespace OpenTemple.Core.Utils
{
    public static class ErrorReporting
    {
        public static List<UnhandledError> Queue {get;} = new List<UnhandledError>();

        public static void ReportException(Exception e)
        {
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