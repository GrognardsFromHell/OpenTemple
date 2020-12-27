using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Logbook
{
    /// <summary>
    /// See the rendering function @ 0x10195d10 for details on how this widget is layed out.
    /// </summary>
    internal class LogbookKeyDetailsUi
    {
        private const PredefinedFont Font = PredefinedFont.ARIAL_10;

        private static readonly TigTextStyle CenteredTextStyle =
            new TigTextStyle(new ColorRect(PackedLinearColorA.White))
            {
                flags = TigTextStyleFlag.TTSF_CENTER,
                kerning = 1,
                tracking = 3
            };

        private static readonly TigTextStyle TextStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            kerning = 1,
            tracking = 3
        };

        private static readonly TigTextStyle TextStyleMuted =
            new TigTextStyle(new ColorRect(new PackedLinearColorA(144, 144, 144, 255)))
            {
                kerning = 1,
                tracking = 3
            };

        public WidgetContainer Container { get; }

        private KeylogEntry _key;

        private LogbookKeyTranslations _translations;

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

        private WidgetContainer _keyDetailsContainer;
        private WidgetContainer _helpContainer;

        private WidgetLegacyText _titleLabel;
        private WidgetLegacyText _acquiredMonth;
        private WidgetLegacyText _acquiredDay;
        private WidgetLegacyText _acquiredTimeOfDay;

        private WidgetLegacyText _neverUsedLabel;
        private WidgetLegacyText _usedMonthLabel;
        private WidgetLegacyText _usedMonth;
        private WidgetLegacyText _usedDayLabel;
        private WidgetLegacyText _usedDay;
        private WidgetLegacyText _usedTimeOfDay;

        private WidgetLegacyText _description;

        public LogbookKeyDetailsUi(LogbookKeyTranslations translations)
        {
            _translations = translations;

            // Created @ 0x10198a55
            Container = new WidgetContainer(new Rectangle(339, 20, 283, 313));
            // logbook_ui_keys_detail_window1.OnBeforeRender += 0x10195d10;
            Container.Name = "logbook_ui_keys_detail_window";

            var separatorLine = new WidgetRectangle();
            separatorLine.Y = 20;
            separatorLine.FixedHeight = 1;
            separatorLine.Pen = new PackedLinearColorA(0xFF909090);
            Container.AddContent(separatorLine);

            _titleLabel = new WidgetLegacyText("", Font,
                CenteredTextStyle);
            _titleLabel.Y = 5;
            Container.AddContent(_titleLabel);

            // Hidden when an actual key is selected
            _helpContainer = new WidgetContainer(new Rectangle(6, 26, 270, 313));
            _helpContainer.AddContent(new WidgetLegacyText(_translations.DetailsHelp, Font, TextStyle));
            Container.Add(_helpContainer);

            _keyDetailsContainer = new WidgetContainer(new Rectangle(6, 26, 270, 313));
            Container.Add(_keyDetailsContainer);

            CreateAcquiredRow();

            CreateUsedRow();

            _description = new WidgetLegacyText("", Font, TextStyle);
            _description.Y = 39;
            _keyDetailsContainer.AddContent(_description);

            Update();
        }

        private void CreateAcquiredRow()
        {
            var acquiredLabel = new WidgetLegacyText(_translations.LabelAcquired, Font, TextStyleMuted);
            acquiredLabel.Y = 0;
            _keyDetailsContainer.AddContent(acquiredLabel);

            var dayLabel = new WidgetLegacyText(_translations.LabelDay, Font, TextStyleMuted);
            dayLabel.X = acquiredLabel.X + acquiredLabel.GetPreferredSize().Width + 16;
            dayLabel.Y = 0;
            _keyDetailsContainer.AddContent(dayLabel);

            _acquiredDay = new WidgetLegacyText("99", Font, TextStyle);
            _acquiredDay.X = dayLabel.X + dayLabel.GetPreferredSize().Width + 1;
            _acquiredDay.Y = 0;
            _keyDetailsContainer.AddContent(_acquiredDay);

            var monthLabel = new WidgetLegacyText(_translations.LabelMonth, Font, TextStyleMuted);
            monthLabel.X = _acquiredDay.X + _acquiredDay.GetPreferredSize().Width + 15;
            monthLabel.Y = 0;
            _keyDetailsContainer.AddContent(monthLabel);

            _acquiredMonth = new WidgetLegacyText("99", Font, TextStyle);
            _acquiredMonth.X = monthLabel.X + monthLabel.GetPreferredSize().Width + 1;
            _acquiredMonth.Y = 0;
            _keyDetailsContainer.AddContent(_acquiredMonth);

            _acquiredTimeOfDay = new WidgetLegacyText("", Font, TextStyle);
            _acquiredTimeOfDay.X = _acquiredMonth.X + _acquiredMonth.GetPreferredSize().Width + 15;
            _acquiredTimeOfDay.Y = 0;
            _keyDetailsContainer.AddContent(_acquiredTimeOfDay);
        }

        private void CreateUsedRow()
        {
            var usedLabel = new WidgetLegacyText(_translations.LabelUsed, Font, TextStyleMuted);
            usedLabel.X = 0;
            usedLabel.Y = 15;
            _keyDetailsContainer.AddContent(usedLabel);

            // The following label will be made invisible if the key has been used
            _neverUsedLabel = new WidgetLegacyText(_translations.NeverUsed, Font, TextStyle);
            _neverUsedLabel.X = usedLabel.X + usedLabel.GetPreferredSize().Width + 16;
            _neverUsedLabel.Y = 15;
            _keyDetailsContainer.AddContent(_neverUsedLabel);

            // The following labels will be made invisible if the key has never been used
            _usedDayLabel = new WidgetLegacyText(_translations.LabelDay, Font, TextStyleMuted);
            _usedDayLabel.X = usedLabel.X + usedLabel.GetPreferredSize().Width + 16;
            _usedDayLabel.Y = 15;
            _keyDetailsContainer.AddContent(_usedDayLabel);

            _usedDay = new WidgetLegacyText("99", Font, TextStyle);
            _usedDay.X = _usedDayLabel.X + _usedDayLabel.GetPreferredSize().Width + 1;
            _usedDay.Y = 15;
            _keyDetailsContainer.AddContent(_usedDay);

            _usedMonthLabel = new WidgetLegacyText(_translations.LabelMonth, Font, TextStyleMuted);
            _usedMonthLabel.X = _usedDay.X + _usedDay.GetPreferredSize().Width + 16;
            _usedMonthLabel.Y = 15;
            _keyDetailsContainer.AddContent(_usedMonthLabel);

            _usedMonth = new WidgetLegacyText("99", Font, TextStyle);
            _usedMonth.X = _usedMonthLabel.X + _usedMonthLabel.GetPreferredSize().Width + 1;
            _usedMonth.Y = 15;
            _keyDetailsContainer.AddContent(_usedMonth);

            _usedTimeOfDay = new WidgetLegacyText("", Font, TextStyle);
            _usedTimeOfDay.X = _usedMonth.X + _usedMonth.GetPreferredSize().Width + 16;
            _usedTimeOfDay.Y = 15;
            _keyDetailsContainer.AddContent(_usedTimeOfDay);
        }

        private void Update()
        {
            _helpContainer.Visible = _key == null;
            _keyDetailsContainer.Visible = _key != null;

            if (_key == null)
            {
                _titleLabel.Text = _translations.NoCurrentKeys;
                return;
            }

            _titleLabel.Text = _key.Title;
            _description.Text = _key.Description;
            _acquiredDay.Text = GameSystems.TimeEvent.GetDayOfMonth(_key.Acquired).ToString();
            _acquiredMonth.Text = GameSystems.TimeEvent.GetMonthOfYear(_key.Acquired).ToString();
            _acquiredTimeOfDay.Text = GameSystems.TimeEvent.FormatTimeOfDay(_key.Acquired);

            if (_key.Used.Time == 0)
            {
                _neverUsedLabel.Visible = true;
                _usedMonthLabel.Visible = false;
                _usedMonth.Visible = false;
                _usedDayLabel.Visible = false;
                _usedDay.Visible = false;
                _usedTimeOfDay.Visible = false;
            }
            else
            {
                _usedDay.Text = GameSystems.TimeEvent.GetDayOfMonth(_key.Used).ToString();
                _usedMonth.Text = GameSystems.TimeEvent.GetMonthOfYear(_key.Used).ToString();
                _usedTimeOfDay.Text = GameSystems.TimeEvent.FormatTimeOfDay(_key.Used);

                _neverUsedLabel.Visible = false;
                _usedMonthLabel.Visible = true;
                _usedMonth.Visible = true;
                _usedDayLabel.Visible = true;
                _usedDay.Visible = true;
                _usedTimeOfDay.Visible = true;
            }
        }
    }
}