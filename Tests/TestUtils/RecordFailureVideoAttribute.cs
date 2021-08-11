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
        private readonly bool _always;

        public RecordFailureVideoAttribute(bool always = false)
        {
            _always = always;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ScreenshotCommandWrapper(command, _always, true);
        }
    }
}