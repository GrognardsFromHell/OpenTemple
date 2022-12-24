using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Logbook;

/// <summary>
/// See the rendering function @ 0x10195d10 for details on how this widget is layed out.
/// </summary>
internal class LogbookKeyDetailsUi
{
    private const string MutedStyle = "logbook-keys-muted";

    public WidgetContainer Container { get; }

    private KeylogEntry _key;

    private readonly LogbookKeyTranslations _translations;

    public KeylogEntry Key
    {
        get => _key;
        set
        {
            if (value != _key)
            {
                _key = value;
                Update();
            }
        }
    }

    private readonly WidgetContainer _keyDetailsContainer;
    private readonly WidgetText _helpText;

    private readonly WidgetText _titleLabel;
    private readonly WidgetText _acquiredLabel;
    private readonly WidgetText _usedLabel;
    private readonly WidgetText _description;

    public LogbookKeyDetailsUi(LogbookKeyTranslations translations)
    {
        _translations = translations;

        var doc = WidgetDoc.Load("ui/logbook/key_details.json");

        // Created @ 0x10198a55
        Container = doc.GetRootContainer();
        // logbook_ui_keys_detail_window1.OnBeforeRender += 0x10195d10;
        Container.Name = "logbook_ui_keys_detail_window";

        var separatorLine = new WidgetRectangle();
        separatorLine.Y = 20;
        separatorLine.FixedHeight = 1;
        separatorLine.Pen = new PackedLinearColorA(0xFF909090);
        Container.AddContent(separatorLine);

        _titleLabel = doc.GetTextContent("title");

        // Hidden when an actual key is selected
        _helpText = doc.GetTextContent("helpText");

        _keyDetailsContainer = doc.GetContainer("keyDetailsContainer");

        _acquiredLabel = doc.GetTextContent("acquiredLabel");
        _usedLabel = doc.GetTextContent("usedLabel");
        _description = doc.GetTextContent("description");

        Update();
    }

    private void Update()
    {
        _helpText.Visible = _key == null;
        _keyDetailsContainer.Visible = _key != null;

        if (_key == null)
        {
            _titleLabel.Text = _translations.NoCurrentKeys;
            return;
        }

        _titleLabel.Text = _key.Title;
        _description.Text = _key.Description;
        _acquiredLabel.Content = CreateAcquiredText(_translations, _key);
        _usedLabel.Content = CreateUsedText();
    }

    internal static InlineElement CreateAcquiredText(LogbookKeyTranslations translations, KeylogEntry key)
    {
        var result = new ComplexInlineElement();
        result.AppendContent(translations.LabelAcquired, MutedStyle);

        // Missing: 16px spacer
        result.AppendContent("\x2001", MutedStyle);

        AppendTimePoint(translations, key.Acquired, result);
        return result;
    }


    private InlineElement CreateUsedText()
    {
        var result = new ComplexInlineElement();

        result.AppendContent(_translations.LabelUsed, MutedStyle);

        // Missing: 16px spacer
        result.AppendContent("\x2001", MutedStyle);

        if (_key.Used.Time == 0)
        {
            result.AppendContent(_translations.NeverUsed);
        }
        else
        {
            AppendTimePoint(_translations, _key.Used, result);
        }

        return result;
    }

    // Append text describing a point in time to an inline element
    private static void AppendTimePoint(LogbookKeyTranslations translations, TimePoint timePoint, IMutableInlineContainer container)
    {
        container.AppendContent(translations.LabelDay, MutedStyle);
        container.AppendContent(GameSystems.TimeEvent.GetDayOfMonth(timePoint).ToString());

        // Missing: 15px spacer
        container.AppendContent("\x2001", MutedStyle);

        container.AppendContent(translations.LabelMonth, MutedStyle);
        container.AppendContent(GameSystems.TimeEvent.GetMonthOfYear(timePoint).ToString());

        // Missing: 16px spacer
        container.AppendContent("\x2001", MutedStyle);

        container.AppendContent(GameSystems.TimeEvent.FormatTimeOfDay(timePoint));
    }

}