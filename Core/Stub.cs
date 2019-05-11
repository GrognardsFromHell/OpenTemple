using System;
using System.Runtime.CompilerServices;

namespace SpicyTemple.Core
{
    public static class Stub
    {
        public static void TODO([CallerFilePath]
            string path = "",
            [CallerLineNumber]
            int lineNumber = -1,
            [CallerMemberName]
            string callerMember = "")
        {
            Console.WriteLine($"{path}:{lineNumber} {callerMember}");
        }
    }
}