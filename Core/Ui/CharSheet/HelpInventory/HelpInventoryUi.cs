using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.HelpInventory;

public class CharSheetHelpUi
{
    private readonly WidgetScrollView _scrollView;

    private readonly WidgetText _textLabel;

    public CharSheetHelpUi()
    {
        var widgetDoc = WidgetDoc.Load("ui/char_help.json");
        _scrollView = (WidgetScrollView) widgetDoc.TakeRootWidget();

        var textContainer = new WidgetContainer
        {
            PixelSize = _scrollView.InnerSize
        };
        _textLabel = new WidgetText();
        textContainer.AddContent(_textLabel);
        _scrollView.Add(textContainer);
        _scrollView.AddStyle("char-help-text");
    }

    [TempleDllLocation(0x10BF0BC0)]
    public bool Shown { get; set; }

    public WidgetBase Container => _scrollView;

    public void Hide()
    {
        Stub.TODO();
        Shown = false;
    }

    [TempleDllLocation(0x101627a0)]
    public void Show()
    {
        Stub.TODO();
        Shown = true;
    }

    [TempleDllLocation(0x10162c00)]
    public void SetHelpText(string text)
    {
        _textLabel.Text = text;
    }
    public void SetHelpText(InlineElement content)
    {
        _textLabel.Content = content;
    }

    [TempleDllLocation(0x101628D0)]
    public InlineElement GetObjectHelp(GameObject obj, GameObject observer)
    {
        return UiSystems.Tooltip.GetObjectDescriptionContent(obj, observer);
    }

    public void ShowItemDescription(GameObject item, GameObject observer)
    {
        var text = UiSystems.CharSheet.Help.GetObjectHelp(item, observer);
        SetHelpText(text);
    }

    public void ClearHelpText() => SetHelpText("");

    [TempleDllLocation(0x10162730)]
    public void Reset()
    {
        ClearHelpText();
    }
}