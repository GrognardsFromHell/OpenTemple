using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

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

        [TempleDllLocation(0x10123220)]
        public string GetItemDescriptionBarter(GameObjectBody observer, GameObjectBody item)
        {
            Stub.TODO();
            return "TODO 0x10123220";
        }

        [TempleDllLocation(0x101237e0)]
        public string GetContainerDescription(GameObjectBody container, GameObjectBody observer)
        {
            Stub.TODO();
            return "TODO 0x101237e0";
        }

        [TempleDllLocation(0x101237e0)]
        public string GetCritterDescription(GameObjectBody critter, GameObjectBody observer)
        {
            Stub.TODO();
            return "TODO 0x101237e0";
        }

        [TempleDllLocation(0x10122dd0)]
        public string GetItemDescription(GameObjectBody item, GameObjectBody observer)
        {
            var result = GameSystems.MapObject.GetDisplayName(item, observer);

            var weight = item.GetInt32(obj_f.item_weight);
            result += "\n\n" + GetString(100) + ": " + weight.ToString(CultureInfo.InvariantCulture);

            switch (item.type)
            {
                case ObjectType.weapon:
                    var critRange = 21 - item.GetInt32(obj_f.weapon_crit_range);
                    critRange -= GameSystems.D20.D20QueryItem(item, D20DispatcherKey.QUE_Weapon_Get_Keen_Bonus);

                    var multiplier = item.GetInt32(obj_f.weapon_crit_hit_chart);

                    var damageDice = Dice.Unpack(item.GetUInt32(obj_f.weapon_damage_dice));
                    var range = item.GetInt32(obj_f.weapon_range);

                    result += GetString(107);
                    result += ": ";
                    result += range.ToString(CultureInfo.InvariantCulture);
                    result += "\n";
                    result += GetString(108);
                    result += ": ";
                    result += damageDice.Count;
                    result += 'd';
                    result += damageDice.Sides;
                    result += ", ";
                    if (critRange != 20)
                    {
                        result += critRange.ToString(CultureInfo.InvariantCulture);
                        result += ',';
                    }

                    result += "20x";
                    result += multiplier.ToString(CultureInfo.InvariantCulture);

                    break;
                case ObjectType.armor:

                    // Armor Class Bonus
                    result += "\n";
                    result += GetString(101);
                    result += ": ";
                    var acBonus = GameSystems.D20.D20QueryItem(item, D20DispatcherKey.QUE_Armor_Get_AC_Bonus);
                    result += $"{acBonus:+#;-#;0}";
                    result += "\n";

                    // Maximum Dexterity Bonus
                    result += GetString(120);
                    result += ": ";
                    var maxDexBonus =
                        GameSystems.D20.D20QueryItem(item, D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus);
                    var maxSpeed = GameSystems.D20.D20QueryItem(item, D20DispatcherKey.QUE_Armor_Get_Max_Speed);

                    if (maxDexBonus == 100)
                    {
                        result += " - ";
                    }
                    else
                    {
                        result += $"{maxDexBonus:+#;-#;0}";
                    }

                    // Maximum Movement Speed
                    result += "   ";
                    result += GetString(122);
                    result += ": ";

                    if (maxSpeed == 100)
                    {
                        result += " - ";
                    }
                    else
                    {
                        result += maxSpeed.ToString(CultureInfo.InvariantCulture);
                    }

                    result += "\n";

                    // Armor Check Penalty
                    result += GetString(121);
                    result += ": ";
                    var armorCheckPenalty = GameSystems.D20.GetArmorSkillCheckPenalty(item);
                    result += armorCheckPenalty.ToString(CultureInfo.InvariantCulture);

                    result += "   ";

                    // Arcane Spell Failure
                    result += GetString(102);
                    result += ": ";
                    var spellFailure = item.GetInt32(obj_f.armor_arcane_spell_failure);
                    result += spellFailure.ToString(CultureInfo.InvariantCulture);
                    result += "%";
                    break;
                case ObjectType.bag:
                    result += "\n";
                    if (GameSystems.Item.BagIsEmpty(observer, item))
                    {
                        result += GetString(106);
                    }
                    else
                    {
                        result += GetString(105);
                        result += " ";
                        result += GetString(106);
                    }

                    break;
                case ObjectType.portal:
                    var portalFlags = item.GetPortalFlags();
                    if (portalFlags.HasFlag(PortalFlag.OPEN))
                    {
                        result += "\n";
                        result += GetString(119);
                    }
                    else if (portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
                    {
                        result += "\n";
                        result += GetString(118);
                        result += " ";
                        result += GetString(117);
                    }
                    else if (portalFlags.HasFlag(PortalFlag.LOCKED))
                    {
                        result += "\n";
                        result += GetString(117);
                    }

                    break;
            }

            if (item.TryGetQuantity(out var quantity) && item.type != ObjectType.armor)
            {
                result += "\n";
                result += GetString(110);
                result += ": ";
                result += quantity.ToString(CultureInfo.InvariantCulture);
            }

            return result;
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
        public TigTextStyle TextStyle { get; }

        public TooltipStyle(string name, PredefinedFont font, PackedLinearColorA color)
        {
            Name = name;
            Font = font;
            Color = color;

            var tooltipStyle = new TigTextStyle();
            tooltipStyle.bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, 204));
            tooltipStyle.shadowColor = new ColorRect(PackedLinearColorA.Black);
            tooltipStyle.textColor = new ColorRect(Color);
            tooltipStyle.flags = TigTextStyleFlag.TTSF_DROP_SHADOW
                                 | TigTextStyleFlag.TTSF_BACKGROUND
                                 | TigTextStyleFlag.TTSF_BORDER;
            tooltipStyle.tracking = 2;
            tooltipStyle.kerning = 2;
            TextStyle = tooltipStyle;
        }
    }
}