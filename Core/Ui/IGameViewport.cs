
using System;
using System.Drawing;
using Avalonia.OpenGL;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Ui
{
    public interface IGameViewport
    {
        void TakeScreenshot(string path, Size size = default);

        LocAndOffsets CenteredOn { get; }

        WorldCamera Camera { get; }

        event Action OnResize;
    }
}