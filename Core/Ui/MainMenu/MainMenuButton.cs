
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

        _normalStyle = new TigTextStyle
        {
            textColor = ColorRect.GradientV("#0064a4", "#01415d"),
            flags = TigTextStyleFlag.Center | TigTextStyleFlag.DropShadow,
            kerning = 0,
            tracking = 10
        };

        _hoverStyle = _normalStyle.Copy();
        _hoverStyle.textColor = ColorRect.GradientV("#01ffff", "#01d0ff");

        _pressedStyle = _normalStyle.Copy();
        _pressedStyle.textColor = ColorRect.GradientV("#eb1510", "#da5b61");
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
        else if (Pressed || ContainsMouse || HasFocus)
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