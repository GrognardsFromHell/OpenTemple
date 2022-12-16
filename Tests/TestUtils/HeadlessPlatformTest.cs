using System;
using System.Diagnostics;
using NUnit.Framework;

namespace OpenTemple.Tests.TestUtils;

/// <summary>
/// Similar to <see cref="HeadlessGameTest"/>, but only initialized the platform systems, not any
/// game or user interface systems.
/// </summary>
[Category("NeedsRealFiles")]
[NonParallelizable]
public class HeadlessPlatformTest
{
    private static HeadlessGameHelper _game;

    public HeadlessGameHelper Game => _game;

    [OneTimeSetUp]
    public void StartGame()
    {
        Trace.Assert(_game == null);
        _game = new HeadlessGameHelper(withGameSystems: false);
    }

    [OneTimeTearDown]
    public void StopGame()
    {
        try
        {
            _game?.Dispose();
        }
        finally
        {
            _game = null;
        }

        // Run all pending finalizers, because sometimes these will crash and
        // if it happens later, it will be harder to associate them with the test
        // that actually caused the problem
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}