using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Transactions;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Stats
{
    public class StatsUiLabel : WidgetButtonBase
    {
        private readonly WidgetLegacyText _label;

        private readonly Func<string> _textGenerator;

        private string _currentText = "";

        public StatsUiLabel(Rectangle rect, TigTextStyle textStyle, Func<string> textGenerator) : base(rect)
        {
            _textGenerator = textGenerator;
            _label = new WidgetLegacyText(_currentText, PredefinedFont.ARIAL_10, textStyle);
            AddContent(_label);
            SetClickHandler(OnClick);
        }

        [TempleDllLocation(0x101c5dd0)]
        private void OnClick()
        {
            GameSystems.Help.ShowTopic("TAG_MONEY");
        }

        public override void Render()
        {
            var text = _textGenerator();
            if (text != _currentText)
            {
                _currentText = text;
                _label.Text = text;
            }

            base.Render();
        }
    }
}