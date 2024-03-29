using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats;

public class StatsCurrencyLabel : WidgetButtonBase
{
    private readonly WidgetText _label;

    private string _currentText = "";

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

        Pos = rect.Location;
        Y = Y - 1; // This is hardcoded in the render-function in ToEE
        PixelSize = rect.Size;

        MoneyType = moneyType;

        _label = new WidgetText(_currentText, "char-ui-stat-money");
        AddContent(_label);
        AddClickListener(ShowHelp);
    }

    [TempleDllLocation(0x101c5dd0)]
    private void ShowHelp()
    {
        GameSystems.Help.ShowTopic("TAG_MONEY");
    }

    public override void Render(UiRenderContext context)
    {
        var text = GetMoneyText();
        if (text != _currentText)
        {
            _currentText = text;
            _label.Text = text;
        }

        base.Render(context);
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

    protected override void HandleTooltip(TooltipEvent e)
    {
        if (ContainsPress)
        {
            return;
        }

        TooltipText = GetMoneyTooltip();
        base.HandleTooltip(e);
    }
}