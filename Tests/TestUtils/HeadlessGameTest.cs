using System.Diagnostics;
using NUnit.Framework;

namespace OpenTemple.Tests.TestUtils
{
    [Category("NeedsRealFiles")]
    public abstract class HeadlessGameTest
    {
        protected static HeadlessGameHelper Game { get; private set; }

        [OneTimeSetUp]
        public static void StartGame()
        {
            Trace.Assert(Game == null);
            Game = new HeadlessGameHelper();
        }

        [OneTimeTearDown]
        public static void StopGame()
        {
            Game?.Dispose();
            Game = null;
        }
    }
}