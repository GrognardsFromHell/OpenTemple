using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace OpenTemple.Tests.TestUtils
{
    /// <summary>
    /// Attempts to take a screenshot automatically after a test has failed, but before any cleanup
    /// tear-down methods have run (to avoid taking a screenshot of an empty map).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TakeFailureScreenshotAttribute : Attribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return new ScreenshotCommandWrapper(command);
        }
    }
}

