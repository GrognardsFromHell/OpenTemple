using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;

namespace OpenTemple.Tests.TestUtils;

/// <summary>
/// Mocked viewport that represents a viewport across the entire headless window. Useful for testing
/// renderers.
/// </summary>
public class MockViewport : IGameViewport
{
    private readonly HeadlessMainWindow _window;

    public MockViewport(WorldCamera camera, HeadlessMainWindow window)
    {
        Camera = camera;
        _window = window;
    }

    public void TakeScreenshot(string path, Size size = default)
    {
        throw new NotImplementedException();
    }

    public WorldCamera Camera { get; }
    public float Zoom { get; } = 1.0f;
    public event Action OnResize;
    public SizeF Size => _window.OffScreenSize;
    public bool IsInteractive => true;
}