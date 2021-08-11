using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace OpenTemple.Tests.TestUtils
{
    /// <summary>
    /// Records every rendered frame and outputs a GIF at the end if the test fails.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RecordFailureVideoAttribute : Attribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command)
        {
            return new ScreenshotCommandWrapper(command, true);
        }
    }
}