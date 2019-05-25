using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui
{
    public class TooltipUi : IDisposable
    {
        private readonly List<TooltipStyle> _styles = new List<TooltipStyle>();

        public TooltipUiRules Rules { get; }

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x10124380)]
        public TooltipUi()
        {
            LoadStyles();

            Rules = new TooltipUiRules(Tig.FS.ReadMesFile("art/interface/tooltip_ui/tooltip_ui_rules.mes"));

            _translations = Tig.FS.ReadMesFile("mes/tooltip_ui_strings.mes");
        }

        [TempleDllLocation(0x10123b30)]
        private void LoadStyles()
        {
            var mesLines = Tig.FS.ReadMesFile("art/interface/tooltip_ui/tooltip_ui_styles.mes");
            var lines = mesLines.OrderBy(kp => kp.Key).Select(kp => kp.Value).ToList();

            for (var i = 0; i < lines.Count; i += 7)
            {
                var name = lines[i];
                var fontName = lines[i + 1];
                var fontSize = int.Parse(lines[i + 2]);
                var alpha = byte.Parse(lines[i + 3]);
                var red = byte.Parse(lines[i + 4]);
                var green = byte.Parse(lines[i + 5]);
                var blue = byte.Parse(lines[i + 6]);

                var font = Tig.Fonts.GetPredefinedFont(fontName, fontSize);
                var color = new PackedLinearColorA(red, green, blue, alpha);

                _styles.Add(new TooltipStyle(name, font, color));
            }
        }

        [TempleDllLocation(0x101238b0)]
        public TooltipStyle GetStyle(int index) => _styles[index];

        [TempleDllLocation(0x10122da0)]
        public string GetString(int key) => _translations[key];

        [TempleDllLocation(0x10122d00)]
        public void Dispose()
        {
        }
    }

    public class TooltipUiRules
    {
        public int MaxWidthClass { get; }
        public int MaxWidthAlignment { get; }
        public int MaxWidthRace { get; }
        public int MaxWidthGod { get; }
        public int MaxWidthSkills { get; }
        public int MaxWidthFeats { get; }
        public int MaxWidthSpells { get; }
        public int MaxWidthAbilities { get; }
        public int MaxWidthCurrency { get; }
        public int MaxWidth3dPortraitButton { get; }
        public int MaxWidth2dPortraitButton { get; }
        public int MaxWidthPaperdollButton { get; }
        public int MaxWidthItemUseButton { get; }
        public int MaxWidthItemDropButton { get; }
        public int MaxWidthCharUiExitButton { get; }
        public int MaxWidthCharUiNameButton { get; }
        public int MaxWidthCharUiClassLevelButton { get; }
        public int MaxWidthCharUiAlignmentGenderRaceButton { get; }
        public int MaxWidthCharUiWorshipButton { get; }

        public int NpcHpStage0Percentage { get; }
        public int NpcHpStage1Percentage { get; }
        public int NpcHpStage2Percentage { get; }
        public int NpcHpStage3Percentage { get; }
        public int NpcHpStage4Percentage { get; }

        public TooltipUiRules(Dictionary<int, string> mesLines)
        {
            // Char UI Stats
            MaxWidthClass = int.Parse(mesLines[100]);
            MaxWidthAlignment = int.Parse(mesLines[101]);
            MaxWidthRace = int.Parse(mesLines[102]);
            MaxWidthGod = int.Parse(mesLines[103]);
            MaxWidthSkills = int.Parse(mesLines[200]);
            MaxWidthFeats = int.Parse(mesLines[201]);
            MaxWidthSpells = int.Parse(mesLines[202]);
            MaxWidthAbilities = int.Parse(mesLines[203]);
            MaxWidthCurrency = int.Parse(mesLines[300]);

            // Char UI General
            MaxWidth3dPortraitButton = int.Parse(mesLines[5000]);
            MaxWidth2dPortraitButton = int.Parse(mesLines[5001]);
            MaxWidthPaperdollButton = int.Parse(mesLines[5002]);
            MaxWidthItemUseButton = int.Parse(mesLines[5003]);
            MaxWidthItemDropButton = int.Parse(mesLines[5004]);
            MaxWidthCharUiExitButton = int.Parse(mesLines[5005]);
            MaxWidthCharUiNameButton = int.Parse(mesLines[5006]);
            MaxWidthCharUiClassLevelButton = int.Parse(mesLines[5007]);
            MaxWidthCharUiAlignmentGenderRaceButton = int.Parse(mesLines[5008]);
            MaxWidthCharUiWorshipButton = int.Parse(mesLines[5009]);

            // NPC HP Stages
            NpcHpStage0Percentage = int.Parse(mesLines[6000]);
            NpcHpStage1Percentage = int.Parse(mesLines[6001]);
            NpcHpStage2Percentage = int.Parse(mesLines[6002]);
            NpcHpStage3Percentage = int.Parse(mesLines[6003]);
            NpcHpStage4Percentage = int.Parse(mesLines[6004]);
        }
    }

    public class TooltipStyle
    {
        public string Name { get; }
        public PredefinedFont Font { get; }
        public PackedLinearColorA Color { get; }

        public TooltipStyle(string name, PredefinedFont font, PackedLinearColorA color)
        {
            Name = name;
            Font = font;
            Color = color;
        }
    }
}