using System;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core;

public static class Stub
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public static void TODO([CallerFilePath]
        string path = "",
        [CallerLineNumber]
        int lineNumber = -1,
        [CallerMemberName]
        string callerMember = "")
    {
        Logger.Warn($"NYI {path}:{lineNumber} {callerMember}");
    }
}