#nullable enable
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.MainMenu;

/// <summary>
/// Custom button used for the main menu.
/// We cannot use the new text rendering because we do not have access to a comparable font.
/// </summary>
public class MainMenuButton : WidgetButtonBase
{
    public string Text
    {
        get => _text;
        set
        {
            _text = Globals.UiAssets.ApplyTranslation(value);
            UpdateBounds();
        }
    }

    private readonly TigTextStyle _normalStyle;
    private readonly TigTextStyle _hoverStyle;
    private readonly TigTextStyle _pressedStyle;
    private string _text = "";

    public MainMenuButton([CallerFilePath]
        string? filePath = null,
        [CallerLineNumber]
        int lineNumber = -1) : base(filePath, lineNumber)
    {
        var defaultSounds = Globals.WidgetButtonStyles.GetStyle("default-sounds");
        SoundMouseEnter = defaultSounds.SoundEnter;
        SoundMouseLeave = defaultSounds.SoundLeave;
        SoundPressed = defaultSounds.SoundDown;
        SoundClicked = defaultSounds.SoundClick;

        _normalStyle = new TigTextStyle()
        {
            textColor = new ColorRect()
            {
                topLeft = PackedLinearColorA.FromHex("#0064a4"),
                topRight = PackedLinearColorA.FromHex("#0064a4"),
                bottomLeft = PackedLinearColorA.FromHex("#01415d"),
                bottomRight = PackedLinearColorA.FromHex("#01415d")
            },
            flags = TigTextStyleFlag.TTSF_CENTER | TigTextStyleFlag.TTSF_DROP_SHADOW,
            leading = 1,
            kerning = 0,
            tracking = 10,
            shadowColor = new ColorRect(PackedLinearColorA.Black)
        };

        _hoverStyle = _normalStyle.Copy();
        _hoverStyle.textColor = new ColorRect()
        {
            topLeft = PackedLinearColorA.FromHex("#01ffff"),
            topRight = PackedLinearColorA.FromHex("#01ffff"),
            bottomLeft = PackedLinearColorA.FromHex("#01d0ff"),
            bottomRight = PackedLinearColorA.FromHex("#01d0ff")
        };

        _pressedStyle = _normalStyle.Copy();
        _pressedStyle.textColor = new ColorRect()
        {
            topLeft = PackedLinearColorA.FromHex("#eb1510"),
            topRight = PackedLinearColorA.FromHex("#eb1510"),
            bottomLeft = PackedLinearColorA.FromHex("#da5b61"),
            bottomRight = PackedLinearColorA.FromHex("#da5b61")
        };
    }

    private void UpdateBounds()
    {
        Tig.Fonts.PushFont(PredefinedFont.SCURLOCK_48);
        var metrics  = Tig.Fonts.MeasureTextSize(
            Text,
            _normalStyle
        );
        Tig.Fonts.PopFont();

        Size = metrics.Size;
    }

    public override void Render()
    {
        var extents = GetContentArea();
        var style = _normalStyle;
        if (ContainsPress)
        {
            style = _pressedStyle;
        }
        else if (Pressed || ContainsMouse)
        {
            style = _hoverStyle;
        }

        Tig.Fonts.PushFont(PredefinedFont.SCURLOCK_48);
        Tig.Fonts.RenderText(
            Text,
            extents,
            style
        );
        Tig.Fonts.PopFont();
    }
}