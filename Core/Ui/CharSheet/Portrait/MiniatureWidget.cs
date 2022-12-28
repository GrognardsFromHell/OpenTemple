using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.CharSheet.Portrait;

public class MiniatureWidget : WidgetButtonBase
{
    public GameObject? Object { get; set; }

    private float _rotationPivot;

    private int _rotationMode = 0;

    private float _rotation;

    // TODO: This should be injected in some other way
    private MapObjectRenderer Renderer => UiSystems.GameView.GetMapObjectRenderer();

    public override void Render(UiRenderContext context)
    {
        base.Render(context);

        if (Object == null)
        {
            return;
        }

        var contentArea = GetContentArea();
        var centerX = contentArea.X + contentArea.Width / 2;
        var centerY = contentArea.Y + contentArea.Height / 2;

        Renderer.RenderObjectInUi(Object, centerX, centerY + 35, _rotation, 1.5f);
    }

    protected override void HandleMouseDown(MouseEvent e)
    {
        if (SetMouseCapture())
        {
            _rotationMode = 1;
        }
    }

    protected override void HandleMouseUp(MouseEvent e)
    {
        ReleaseMouseCapture();
        _rotationMode = 0;
    }

    protected override void HandleMouseMove(MouseEvent e)
    {
        if (_rotationMode == 1)
        {
            _rotationMode = 2;
            _rotationPivot = e.X;
        }
        else if (_rotationMode == 2)
        {
            var deltaX = e.X - _rotationPivot;
            _rotation -= deltaX * Angles.ToRadians(1.8f);
            _rotationPivot = e.X;
        }
    }
}