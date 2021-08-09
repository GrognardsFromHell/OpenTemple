using System.Diagnostics;
using NUnit.Framework;

namespace OpenTemple.Tests.TestUtils
{
    public abstract class HeadlessGameTest
    {
        protected static HeadlessGameHelper Game { get; private set; }

        [OneTimeSetUp]
        public static void StartGame()
        {
            Debug.Assert(Game == null);
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