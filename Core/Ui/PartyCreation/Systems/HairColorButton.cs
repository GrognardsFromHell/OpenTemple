using System.Collections;
using System.Diagnostics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

public class HairColorButton : WidgetButtonBase
{
    private static readonly PackedLinearColorA OutlinePressed = new(0xFFAAE9FF);
    private static readonly PackedLinearColorA OutlineHovered = new(0xFF376FAA);
    private static readonly PackedLinearColorA OutlineSelected = new(0xFF1AC3FF);
    private static readonly PackedLinearColorA OutlineDefault = new(0xFF43576E);

    public bool Selected { get; set; }

    public PackedLinearColorA HairColor
    {
        set
        {
            _colorRect.Brush = new Brush(value);
            HairStyleTexture = null;
        }
    }

    public string HairStyleTexture
    {
        set
        {
            _stylePreview.Visible = value != null;
            if (value != null)
            {
                _colorRect.Brush = null;
                _stylePreview.SetTexture(value);
            }
        }
    }

    private readonly WidgetRectangle _colorRect;

    private readonly WidgetImage _stylePreview;

    public HairColorButton()
    {
        _stylePreview = new WidgetImage();
        _stylePreview.Visible = false;
        AddContent(_stylePreview);

        _colorRect = new WidgetRectangle();
        AddContent(_colorRect);
    }

    public override void Render()
    {
        _colorRect.Pen = ButtonState switch
        {
            LgcyButtonState.Down => OutlinePressed,
            LgcyButtonState.Hovered => OutlineHovered,
            _ when Selected => OutlineSelected,
            _ => OutlineDefault
        };

        base.Render();
    }
}