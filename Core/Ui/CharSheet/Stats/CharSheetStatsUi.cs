using System;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Stats
{
    public class CharSheetStatsUi : IDisposable
    {
        public WidgetContainer Container { get; }

        private WidgetLegacyText _moneyPlatinum;
        private WidgetLegacyText _moneyGold;
        private WidgetLegacyText _moneySilver;
        private WidgetLegacyText _moneyCopper;

        [TempleDllLocation(0x101ccce0)]
        public CharSheetStatsUi(Rectangle mainWindowRectangle)
        {
            Stub.TODO();

            var uiParams = new StatsUiParams(
                mainWindowRectangle,
                Tig.FS.ReadMesFile("art/interface/char_ui/char_stats_ui/1_char_stats_ui.mes"),
                Tig.FS.ReadMesFile("art/interface/char_ui/char_stats_ui/1_char_stats_ui_textures.mes"),
                Tig.FS.ReadMesFile("mes/1_char_stats_ui_text.mes")
            );

            Container = new WidgetContainer(uiParams.MainWindow);
            Container.SetVisible(false);

            var moneyStyle = new TigTextStyle();
            moneyStyle.shadowColor = new ColorRect(PackedLinearColorA.Black);
            moneyStyle.textColor = new ColorRect(uiParams.FontNormalColor);
            moneyStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW;
            moneyStyle.kerning = 1;
            moneyStyle.tracking = 3;

            Container.Add(new StatsUiLabel(uiParams.PlatinumButton, moneyStyle, () => GetMoneyText(Stat.money_pp)));
            Container.Add(new StatsUiLabel(uiParams.GoldButton, moneyStyle, () => GetMoneyText(Stat.money_gp)));
            Container.Add(new StatsUiLabel(uiParams.SilverButton, moneyStyle, () => GetMoneyText(Stat.money_sp)));
            Container.Add(new StatsUiLabel(uiParams.CopperButton, moneyStyle, () => GetMoneyText(Stat.money_cp)));
        }

        private string GetMoneyText(Stat stat)
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            if (critter == null)
            {
                return "";
            }

            var amount = GameSystems.Stat.StatLevelGet(critter, stat);
            return $" {amount:D4}";
        }

        [TempleDllLocation(0x101c8dd0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101be430)]
        public void Show()
        {
            Container.SetVisible(true);
            Stub.TODO();
        }

        [TempleDllLocation(0x101be460)]
        public void Hide()
        {
            Stub.TODO();
        }

        public void Reset()
        {
        }
    }
}