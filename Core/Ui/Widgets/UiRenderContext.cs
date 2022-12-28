using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Ui.Widgets;

public class UiRenderContext : IUiRenderContext
{
    private readonly RenderingDevice _device;
    private readonly ShapeRenderer2d _shapeRenderer2d;
    private readonly List<RectangleF> _scissorRects = new();

    public float UiScale { get; set; } = 1;

    public UiRenderContext(RenderingDevice device, ShapeRenderer2d shapeRenderer2d)
    {
        _device = device;
        _shapeRenderer2d = shapeRenderer2d;
    }

    public void PushScissorRect(RectangleF rect)
    {
        _scissorRects.Add(rect);
        _device.SetUiScissorRect(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void PopScissorRect()
    {
        _scissorRects.RemoveAt(_scissorRects.Count - 1);
        if (_scissorRects.Count > 0)
        {
            var lastRect = _scissorRects[^1];
            _device.SetUiScissorRect(lastRect.X, lastRect.Y, lastRect.Width, lastRect.Height);
        }
        else
        {
            _device.ResetScissorRect();
        }
    }
}