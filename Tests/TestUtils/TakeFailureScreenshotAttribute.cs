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
        private readonly bool _always;

        public TakeFailureScreenshotAttribute(bool always = false)
        {
            _always = always;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ScreenshotCommandWrapper(command, _always);
        }
    }
}