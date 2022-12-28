using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Logbook;

/// <summary>
/// See primarily function 0x101956a0 for details on how the widget is laid out.
/// </summary>
internal class LogbookKeyButton : WidgetButtonBase
{
        
    private KeylogEntry? _key;

    private readonly WidgetText _title;

    private readonly WidgetText _acquiredLabel;

    private readonly WidgetRectangle _outerRectangle;

    private readonly LogbookKeyTranslations _translations;

    public bool IsSelected { get; set; }

    public LogbookKeyButton(LogbookKeyTranslations translations, Rectangle rect) : base(rect)
    {
        AddStyle("logbook-key-button");

        _translations = translations;

        _title = new WidgetText();
        _title.Text = " ";
        _title.X = 5;
        _title.Y = 2;
        _title.FixedSize = new Size(rect.Width, _title.GetPreferredSize().Height);
        AddContent(_title);

        var currentY = _title.GetPreferredSize().Height + 1;

        _acquiredLabel = new WidgetText();
        _acquiredLabel.X = 5;
        _acquiredLabel.Y = currentY;
        AddContent(_acquiredLabel);

        var innerRectangle = new WidgetRectangle();
        innerRectangle.X = 1;
        innerRectangle.Y = 1;
        innerRectangle.FixedSize = new Size(rect.Width - 2, rect.Height - 2);
        innerRectangle.Pen = new PackedLinearColorA(0xFF909090);
        AddContent(innerRectangle);

        _outerRectangle = new WidgetRectangle();
        _outerRectangle.Pen = PackedLinearColorA.White;
        AddContent(_outerRectangle);

    }

    public KeylogEntry? Key
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
                Update();
            }
        }
    }

    public override void Render(UiRenderContext context)
    {
        if (IsSelected)
        {
            _outerRectangle.Pen = new PackedLinearColorA(0xFF1AC3FF);
        }
        else if (ContainsMouse && !Pressed)
        {
            _outerRectangle.Pen = PackedLinearColorA.White;
        }
        else
        {
            _outerRectangle.Pen = new PackedLinearColorA(0, 0, 0, 0);
        }

        if (_key == null)
        {
            return; // Don't render when no key is assigned
        }

        base.Render(context);
    }

    private void Update()
    {
        Visible = _key != null;

        if (_key != null)
        {
            _title.Text = _key.Title;
            _acquiredLabel.Content = LogbookKeyDetailsUi.CreateAcquiredText(_translations, _key);
        }
    }

}