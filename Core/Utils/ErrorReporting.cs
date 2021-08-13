using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace OpenTemple.Core.Utils
{
    public static class ErrorReporting
    {
        public static List<UnhandledError> Queue { get; } = new List<UnhandledError>();

        public static bool DisableErrorReporting { get; set; }

        [MustUseReturnValue]
        public static bool ReportException(Exception e)
        {
            if (DisableErrorReporting || Debugger.IsAttached)
            {
                return false;
            }
            Queue.Add(new UnhandledError(e));
            return true;
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