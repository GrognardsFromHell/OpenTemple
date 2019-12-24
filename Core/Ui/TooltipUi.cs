using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Ui
{
    public enum InjuryLevel
    {
        Uninjured = 0,
        SlightlyInjured = 1,
        Injured = 2,
        BadlyInjured = 3,
        NearDeath = 4,
        DeadOrDying = -1
    }

    public class TooltipUi : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly List<TooltipStyle> _styles = new List<TooltipStyle>();

        [TempleDllLocation(0x10301384)]
        [TempleDllLocation(0x101FA870)]
        [TempleDllLocation(0x101FA880)]
        [TempleDllLocation(0x101FA890)]
        public bool TooltipsEnabled { get; set; } = true;

        [TempleDllLocation(0x101FA8A0)]
        public TimeSpan TooltipDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        public TooltipUiRules Rules { get; }

        public TooltipStyle DefaultStyle => _styles[0];

        private Dictionary<int, string> _translations;

        [TempleDllLocation(0x10124380)]
        public TooltipUi()
        {
            LoadStyles();

            Rules = new TooltipUiRules(Tig.FS.ReadMesFile("art/interface/tooltip_ui/tooltip_ui_rules.mes"));

            _translations = Tig.FS.ReadMesFile("mes/tooltip_ui_strings.mes");

            AddActionBarTooltipStyle();
        }

        private void AddActionBarTooltipStyle()
        {
            var style = new TooltipStyle("action-bar", PredefinedFont.ARIAL_10, new PackedLinearColorA(0xFF99FF99));
            style.TextStyle.tracking = 5;
            _styles.Add(style);
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

        public TooltipStyle GetStyle(string name)
        {
            var style = _styles.Find(s => s.Name == name);
            if (style == null)
            {
                Logger.Warn("Unknown tooltip style '{0}' was used.", name);
                return GetStyle(0);
            }
            return style;
        }

        [TempleDllLocation(0x10122da0)]
        public string GetString(int key) => _translations[key];

        [TempleDllLocation(0x10122d00)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10123a80)]
        public InjuryLevel GetInjuryLevel(GameObjectBody obj)
        {
            var currentHp = obj.GetStat(Stat.hp_current);
            var maxHp = obj.GetStat(Stat.hp_max);

            // We don't want these two cases to be affected by the rounding that's gonna happen
            if (currentHp <= 0)
            {
                return InjuryLevel.DeadOrDying;
            }

            if (currentHp >= maxHp)
            {
                return InjuryLevel.Uninjured;
            }

            var hpPercentage = (int) (currentHp * 100.0f / maxHp);

            if (hpPercentage < Rules.NpcHpStage3Percentage)
            {
                return InjuryLevel.NearDeath;
            }

            if (hpPercentage < Rules.NpcHpStage2Percentage)
            {
                return InjuryLevel.BadlyInjured;
            }

            if (hpPercentage < Rules.NpcHpStage1Percentage)
            {
                return InjuryLevel.Injured;
            }

            return InjuryLevel.SlightlyInjured;
        }

        [TempleDllLocation(0x10124270)]
        private string GetInjuryLevelDescription(GameObjectBody critter)
        {
            var injuryLevel = GetInjuryLevel(critter);
            switch (injuryLevel)
            {
                case InjuryLevel.Uninjured:
                    return _translations[200];
                case InjuryLevel.SlightlyInjured:
                    return _translations[201];
                case InjuryLevel.Injured:
                    return _translations[202];
                case InjuryLevel.BadlyInjured:
                    return _translations[203];
                // There is no text for death/dying associated with the injury level
                case InjuryLevel.DeadOrDying:
                case InjuryLevel.NearDeath:
                    return _translations[204];
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [TempleDllLocation(0x10138e20)]
        [TempleDllLocation(0x10141a50)]
        public PackedLinearColorA GetInjuryLevelColor(InjuryLevel injuryLevel)
        {
            switch (injuryLevel)
            {
                case InjuryLevel.SlightlyInjured:
                    return new PackedLinearColorA(0xFF00FF00);
                case InjuryLevel.Injured:
                    return new PackedLinearColorA(0xFF7FFF00);
                case InjuryLevel.BadlyInjured:
                    return new PackedLinearColorA(0xFFFF7F00);
                case InjuryLevel.NearDeath:
                    return new PackedLinearColorA(0xFFFF0000);
                default:
                    return PackedLinearColorA.White;
            }
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
            var description = new StringBuilder();

            // Start by appending the name
            description.Append(GameSystems.MapObject.GetDisplayName(container, observer));

            if (container.GetInt32(obj_f.container_inventory_num) <= 0)
            {
                description.Append("\n\n");
                description.Append(_translations[106]);
            }
            else
            {
                description.Append("\n\n");
                description.Append(_translations[105]);
                description.Append(' ');
                description.Append(_translations[106]);
            }

            return description.ToString();
        }

        [TempleDllLocation(0x101243b0)]
        public string GetCritterDescription(GameObjectBody critter, GameObjectBody observer)
        {
            var description = new StringBuilder();

            // Start by appending the name
            description.Append(GameSystems.MapObject.GetDisplayName(critter, observer));

            description.Append("\n\n");
            AppendHitPointDescription(critter, description);

            if (GameSystems.Party.IsInParty(critter))
            {
                var level = critter.GetStat(Stat.level);
                if (level > 0)
                {
                    description.Append('\n');
                    description.Append(_translations[104]); // Level
                    description.Append(": ");
                    description.Append(level);
                }

                if (GameSystems.Combat.IsCombatActive())
                {
                    description.Append('\n');
                    description.Append(_translations[111]); // Initiative
                    description.Append(": ");
                    var initiative = GameSystems.D20.Initiative.GetInitiative(critter);
                    description.Append(initiative);
                }
            }

            var dispIo = DispIoTooltip.Default;
            critter.GetDispatcher()?.Process(DispatcherType.Tooltip, D20DispatcherKey.NONE, dispIo);

            foreach (var line in dispIo.Lines)
            {
                description.Append('\n');
                description.Append(line);
            }

            return description.ToString();
        }

        private void AppendHitPointDescription(GameObjectBody critter, StringBuilder description)
        {
            var subdualDamage = critter.GetStat(Stat.subdual_damage);
            var currentHp = critter.GetStat(Stat.hp_current);
            var maxHp = critter.GetStat(Stat.hp_max);

            if (critter.IsPC())
            {
                description.Append(_translations[103]);
                description.Append(": @1"); // Switches to HP text color
                description.Append(currentHp);
                description.Append("@0/");
                description.Append(maxHp);

                if (subdualDamage > 0)
                {
                    description.Append("@2(");
                    description.Append(subdualDamage);
                    description.Append(")@0");
                }
            }
            else if (GameSystems.Critter.IsDeadNullDestroyed(critter) || currentHp <= 0)
            {
                description.Append(_translations[108]);
                description.Append(": @1");
                description.Append(maxHp - currentHp);
                description.Append("@0");
            }
            else
            {
                description.Append("@1");
                description.Append(GetInjuryLevelDescription(critter));
                description.Append("@0");

                if (subdualDamage <= 0)
                {
                    // Show the amount of damage dealt so far
                    if (currentHp < maxHp)
                    {
                        description.Append('\n');
                        description.Append(_translations[108]); // "Damage"
                        description.Append(": @1");
                        description.Append(maxHp - currentHp);
                        description.Append("@0");
                    }
                }
                else
                {
                    if (subdualDamage >= maxHp)
                    {
                        description.Append('\n');
                        description.Append(_translations[108]); // "Damage"
                        description.Append(": @2(");
                        description.Append(subdualDamage);
                        description.Append(")@0");
                    }
                    else
                    {
                        description.Append('\n');
                        description.Append(_translations[108]); // "Damage"
                        description.Append(": @1");
                        description.Append(maxHp - currentHp);
                        description.Append("@0 @2(");
                        description.Append(subdualDamage);
                        description.Append(")@0");
                    }
                }
            }
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

        public void ClampTooltipToScreen(ref Rectangle extents)
        {
            const int margin = 3;
            var screenSize = Globals.UiManager.ScreenSize;

            if (extents.X < margin)
            {
                extents.X = margin;
            }

            if (extents.Y < margin)
            {
                extents.Y = margin;
            }

            if (extents.Right > screenSize.Width - margin)
            {
                extents.X = screenSize.Width - extents.Width - margin;
            }

            if (extents.Bottom > screenSize.Height - margin)
            {
                extents.Y = screenSize.Height - extents.Height - margin;
            }
        }

        public string GetObjectDescription(GameObjectBody obj)
        {
            return GetObjectDescription(obj, GameSystems.Party.GetConsciousLeader());
        }

        [TempleDllLocation(0x101247a0)]
        public string GetObjectDescription(GameObjectBody obj, GameObjectBody observer)
        {
            if (obj.type.IsEquipment())
            {
                return UiSystems.Tooltip.GetItemDescription(obj, observer);
            }
            else if (obj.type == ObjectType.container)
            {
                return UiSystems.Tooltip.GetContainerDescription(obj, observer);
            }
            else if (obj.IsCritter())
            {
                return UiSystems.Tooltip.GetCritterDescription(obj, observer);
            }
            else
            {
                return "";
            }
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