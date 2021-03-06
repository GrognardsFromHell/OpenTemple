using System.Drawing;
using SharpDX.Win32;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Logbook
{
    /// <summary>
    /// See primarily function 0x101956a0 for details on how the widget is layed out.
    /// </summary>
    internal class LogbookKeyButton : WidgetButtonBase
    {
        
        private const PredefinedFont Font = PredefinedFont.ARIAL_10;

        private static readonly TigTextStyle TextStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_TRUNCATE,
            kerning = 1,
            tracking = 3
        };
        private static readonly TigTextStyle TextStyleMuted = new TigTextStyle(new ColorRect(new PackedLinearColorA(144, 144, 144, 255)))
        {
            flags = TigTextStyleFlag.TTSF_TRUNCATE,
            kerning = 1,
            tracking = 3
        };

        private KeylogEntry _key;

        private readonly WidgetLegacyText _title;

        private readonly WidgetLegacyText _acquiredDay;

        private readonly WidgetLegacyText _acquiredMonth;

        private readonly WidgetLegacyText _acquiredTimeOfDay;

        private readonly WidgetRectangle _outerRectangle;

        public bool IsSelected { get; set; }

        public LogbookKeyButton(LogbookKeyTranslations translations, Rectangle rect) : base(rect)
        {
            _title = new WidgetLegacyText(" ", Font, TextStyle);
            _title.SetY(2);
            _title.FixedSize = new Size(rect.Width, _title.GetPreferredSize().Height);
            AddContent(_title);

            var currentY = _title.GetPreferredSize().Height + 1;

            var acquiredLabel = new WidgetLegacyText("  " + translations.LabelAcquired, Font, TextStyleMuted);
            acquiredLabel.SetY(currentY);
            AddContent(acquiredLabel);

            var dayLabel = new WidgetLegacyText("  " + translations.LabelDay, Font, TextStyleMuted);
            dayLabel.SetX(acquiredLabel.GetX() + acquiredLabel.GetPreferredSize().Width);
            dayLabel.SetY(currentY);
            AddContent(dayLabel);

            _acquiredDay = new WidgetLegacyText("99", Font, TextStyle);
            _acquiredDay.SetX(dayLabel.GetX() + dayLabel.GetPreferredSize().Width + 1);
            _acquiredDay.SetY(currentY);
            AddContent(_acquiredDay);

            var monthLabel = new WidgetLegacyText(translations.LabelMonth, Font, TextStyleMuted);
            monthLabel.SetX(_acquiredDay.GetX() + _acquiredDay.GetPreferredSize().Width + 15);
            monthLabel.SetY(currentY);
            AddContent(monthLabel);

            _acquiredMonth = new WidgetLegacyText("99", Font, TextStyle);
            _acquiredMonth.SetX(monthLabel.GetX() + monthLabel.GetPreferredSize().Width + 1);
            _acquiredMonth.SetY(currentY);
            AddContent(_acquiredMonth);

            _acquiredTimeOfDay = new WidgetLegacyText("", Font, TextStyle);
            _acquiredTimeOfDay.SetX(_acquiredMonth.GetX() + 15);
            _acquiredTimeOfDay.SetY(currentY);
            AddContent(_acquiredTimeOfDay);

            var innerRectangle = new WidgetRectangle();
            innerRectangle.SetX(1);
            innerRectangle.SetY(1);
            innerRectangle.FixedSize = new Size(rect.Width - 2, rect.Height - 2);
            innerRectangle.Pen = new PackedLinearColorA(0xFF909090);
            AddContent(innerRectangle);

            _outerRectangle = new WidgetRectangle();
            _outerRectangle.Pen = PackedLinearColorA.White;
            AddContent(_outerRectangle);

        }

        public KeylogEntry Key
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

        public override void Render()
        {
            if (IsSelected)
            {
                _outerRectangle.Pen = new PackedLinearColorA(0xFF1AC3FF);
            }
            else if (ButtonState == LgcyButtonState.Hovered)
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

            base.Render();
        }

        private void Update()
        {
            Visible = _key != null;

            if (_key != null)
            {
                // TODO: Instead of left-padding, we should just move the label to the right...
                _title.Text = "  " + _key.Title;
                _acquiredDay.Text =  GameSystems.TimeEvent.GetMonthOfYear(_key.Acquired).ToString();
                _acquiredMonth.Text =  GameSystems.TimeEvent.GetDayOfMonth(_key.Acquired).ToString();
                _acquiredTimeOfDay.Text = GameSystems.TimeEvent.FormatTimeOfDay(_key.Acquired);
            }
        }

    }
}