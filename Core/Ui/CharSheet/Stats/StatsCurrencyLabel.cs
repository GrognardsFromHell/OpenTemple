using System;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats
{
    public class StatsCurrencyLabel : WidgetButtonBase
    {
        private readonly WidgetLegacyText _label;

        private string _currentText = "";

        private readonly WidgetLegacyText _tooltipLabel;

        public MoneyType MoneyType { get; }

        private readonly string _tooltipSuffix;

        public StatsCurrencyLabel(StatsUiParams uiParams, MoneyType moneyType)
        {
            Rectangle rect;
            switch (moneyType)
            {
                case MoneyType.Copper:
                    rect = uiParams.CopperButton;
                    _tooltipSuffix = UiSystems.Tooltip.GetString(1704);
                    break;
                case MoneyType.Silver:
                    rect = uiParams.SilverButton;
                    _tooltipSuffix = UiSystems.Tooltip.GetString(1703);
                    break;
                case MoneyType.Gold:
                    rect = uiParams.GoldButton;
                    _tooltipSuffix = UiSystems.Tooltip.GetString(1702);
                    break;
                case MoneyType.Platinum:
                    rect = uiParams.PlatinumButton;
                    _tooltipSuffix = UiSystems.Tooltip.GetString(1701);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moneyType), moneyType, null);
            }

            SetPos(rect.Location);
            Y = Y - 1; // This is hardcoded in the render-function in ToEE
            SetSize(rect.Size);

            var textStyle = new TigTextStyle
            {
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                textColor = new ColorRect(uiParams.FontNormalColor),
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
                kerning = 1,
                tracking = 3
            };

            MoneyType = moneyType;

            _label = new WidgetLegacyText(_currentText, PredefinedFont.ARIAL_10, textStyle);
            AddContent(_label);
            SetClickHandler(OnClick);

            var tooltipStyle = new TigTextStyle();
            tooltipStyle.bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, 204));
            tooltipStyle.shadowColor = new ColorRect(PackedLinearColorA.Black);
            tooltipStyle.textColor = new ColorRect(PackedLinearColorA.White);
            tooltipStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW
                                 | TigTextStyleFlag.TTSF_BACKGROUND
                                 | TigTextStyleFlag.TTSF_BORDER;
            tooltipStyle.tracking = 2;
            tooltipStyle.kerning = 2;

            _tooltipLabel = new WidgetLegacyText("", PredefinedFont.ARIAL_10, tooltipStyle);
        }

        [TempleDllLocation(0x101c5dd0)]
        private void OnClick()
        {
            GameSystems.Help.ShowTopic("TAG_MONEY");
        }

        public override void Render()
        {
            var text = GetMoneyText();
            if (text != _currentText)
            {
                _currentText = text;
                _label.Text = text;
            }

            base.Render();
        }

        private string GetMoneyText()
        {
            var amount = GetMoneyAmount();
            return $" {amount:D4}";
        }

        private string GetMoneyTooltip()
        {
            var amount = GetMoneyAmount();
            return $"{amount} {_tooltipSuffix}";
        }

        private int GetMoneyAmount()
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            if (critter == null)
            {
                return 0;
            }

            switch (MoneyType)
            {
                case MoneyType.Copper:
                    return GameSystems.Stat.StatLevelGet(critter, Stat.money_cp);
                case MoneyType.Silver:
                    return GameSystems.Stat.StatLevelGet(critter, Stat.money_sp);
                case MoneyType.Gold:
                    return GameSystems.Stat.StatLevelGet(critter, Stat.money_gp);
                case MoneyType.Platinum:
                    return GameSystems.Stat.StatLevelGet(critter, Stat.money_pp);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void RenderTooltip(int x, int y)
        {
            if (ButtonState == LgcyButtonState.Down)
            {
                return;
            }

            _tooltipLabel.Text = GetMoneyTooltip();

            var preferredSize = _tooltipLabel.GetPreferredSize();
            var contentArea = new Rectangle(x, y - preferredSize.Height, preferredSize.Width, preferredSize.Height);
            _tooltipLabel.SetContentArea(contentArea);
            _tooltipLabel.Render();
        }
    }
}