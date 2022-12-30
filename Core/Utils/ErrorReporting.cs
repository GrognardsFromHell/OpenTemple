using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace OpenTemple.Core.Utils;

public static class ErrorReporting
{
    public static List<UnhandledError> Queue { get; } = new();

    public static bool DisableErrorReporting { get; set; }

    public static void RunSafe(Action action)
    {
        if (Debugger.IsAttached)
        {
            action();
        }
        else
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (!ReportException(e))
                {
                    throw;
                }
            }
        }
    }
    
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