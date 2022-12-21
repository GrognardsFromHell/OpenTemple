
using System;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Ui;

public interface IGameViewport
{
    void TakeScreenshot(string path, Size size = default);

    LocAndOffsets CenteredOn => ScreenToTile(Size.Width / 2, Size.Height / 2);

    WorldCamera Camera { get; }

    float Zoom { get; }

    event Action OnResize;

    Size Size { get; }
    
    bool IsInteractive { get; }

    [TempleDllLocation(0x10029300)]
    [TempleDllLocation(0x1002A1E0)]
    LocAndOffsets ScreenToTile(float screenX, float screenY)
    {
        return Camera.ScreenToTile(screenX / Zoom, screenY / Zoom);
    }

    Vector3 ScreenToWorld(float screenX, float screenY)
    {
        return Camera.ScreenToWorld(screenX / Zoom, screenY / Zoom);
    }

    Ray3d GetPickRay(float screenX, float screenY)
    {
        return Camera.GetPickRay(screenX / Zoom, screenY / Zoom);
    }

    Vector2 WorldToScreen(Vector3 worldPos)
    {
        return Camera.WorldToScreenUi(worldPos) * Zoom;
    }
}