using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.Spells
{
    internal static class SpellFileParser
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        internal static SpellEntry Parse(int spellEnum, string path)
        {
            using var reader = Tig.FS.OpenTextReader(path, Encoding.Default);
            return Parse(spellEnum, path, reader);
        }

        [TempleDllLocation(0x1007b320)]
        internal static SpellEntry Parse(int spellEnum, string path, TextReader reader)
        {
            var entry = new SpellEntry();
            entry.spellEnum = spellEnum;

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                if (!ParseSpellLine(line, out var key, out var value))
                {
                    Logger.Warn("{0}: Couldn't parse line '{1}'", path, line);
                    continue;
                }

                switch (key)
                {
                    case SpellLineKey.School:
                        if (ParseEnum(Schools, value, out var schoolOfMagic))
                        {
                            entry.spellSchoolEnum = schoolOfMagic;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unknown school of magic: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.Subschool:
                        if (ParseEnum(SubSchools, value, out var subSchoolOfMagic))
                        {
                            entry.spellSubSchoolEnum = subSchoolOfMagic;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unknown sub-school of magic: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.Descriptor:
                        if (ParseEnum(Descriptors, value, out var descriptor))
                        {
                            entry.spellDescriptorBitmask |= descriptor;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unknown spell descriptor: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.Level:
                        if (ParseSpellListEntry(value, out var spellListEntry))
                        {
                            entry.spellLvls.Add(spellListEntry);
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable spell list entry: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.Component:
                        if (ParseSpellComponent(value, out var spellComponent, out var argument))
                        {
                            entry.spellComponentBitmask |= spellComponent;
                            if (spellComponent == SpellComponent.Material)
                            {
                                entry.costGp = argument;
                            }
                            else if (spellComponent == SpellComponent.Experience)
                            {
                                entry.costXp = argument;
                            }
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable spell list entry: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.CastingTime:
                        if (ParseEnum(CastingTimes, value, out var castingTime))
                        {
                            entry.castingTimeType = castingTime;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable casting time: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.Range:
                        if (ParseSpellRange(value, out var spellRange, out var rangeArgument))
                        {
                            entry.spellRangeType = spellRange;
                            if (spellRange == SpellRangeType.SRT_Specified)
                            {
                                entry.spellRange = rangeArgument;
                            }
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable spell range entry: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.SavingThrow:
                        if (ParseEnum(SavingThrows, value, out var savingThrow))
                        {
                            entry.savingThrowType = savingThrow;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable spell saving throw entry: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.SpellResistance:
                        if (ParseEnum(SpellResistanceTypes, value, out var spellResistanceType))
                        {
                            entry.spellResistanceCode = spellResistanceType;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable spell resistance type: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.Projectile:
                        if (ParseEnum(YesNo, value, out var projectileFlag))
                        {
                            entry.projectileFlag = projectileFlag;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable projectile: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.FlagsTarget:
                        if (ParseEnum(TargetFlags, value, out var targetFlag))
                        {
                            entry.flagsTargetBitmask |= targetFlag;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable target flags: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.IncFlagsTarget:
                        if (ParseEnum(TargetIncludeFlags, value, out var targetIncFlag))
                        {
                            entry.incFlagsTargetBitmask |= targetIncFlag;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable target include flags: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.ExcFlagsTarget:
                        if (ParseEnum(TargetIncludeFlags, value, out var targetExcFlag))
                        {
                            entry.excFlagsTargetBitmask |= targetExcFlag;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable target exclude flags: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.ModeTarget:
                        if (ParseEnum(PickerModes, value, out var pickerMode))
                        {
                            entry.modeTargetSemiBitmask |= pickerMode;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable picker mode: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.MinTarget:
                        if (int.TryParse(value, out var minTargets))
                        {
                            entry.minTarget = minTargets;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable min targets: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.MaxTarget:
                        if (int.TryParse(value, out var maxTargets))
                        {
                            entry.maxTarget = maxTargets;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable max targets: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.RadiusTarget:
                        if (ParseSpellRange(value, out var targetSpellRangeType, out _))
                        {
                            entry.radiusTarget = -(int) targetSpellRangeType;
                        }
                        else if (int.TryParse(value, out var targetSpellRange))
                        {
                            entry.radiusTarget = targetSpellRange;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable spell range entry: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.DegreesTarget:
                        if (int.TryParse(value, out var degreesTarget))
                        {
                            entry.degreesTarget = degreesTarget;
                        }
                        else
                        {
                            Logger.Warn("{0}: Unparsable target degrees: '{1}'", path, line);
                        }

                        break;
                    case SpellLineKey.AiType:
                        if (ParseEnum(AiSpellTypes, value, out var aiSpellType))
                        {
                            entry.aiTypeBitmask |= aiSpellType;
                        }
                        else
                        {
                            Logger.Warn("{0} unknown AI spell: {1}", path, line);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return entry;
        }

        private static bool ParseSpellRange(ReadOnlySpan<char> value, out SpellRangeType range, out int argument)
        {
            argument = 0;
            range = default;

            var argSep = value.IndexOf(' ');
            if (argSep != -1)
            {
                if (!int.TryParse(value.Slice(argSep + 1).Trim(), out argument))
                {
                    return false;
                }

                value = value.Slice(0, argSep).Trim();
            }

            return ParseEnum(SpellRanges, value, out range);
        }

        private static bool ParseSpellComponent(ReadOnlySpan<char> value, out SpellComponent component,
            out int argument)
        {
            argument = 0;
            component = default;

            // Some material components have an optional argument
            var argSep = value.IndexOf(' ');
            if (argSep != -1)
            {
                if (!int.TryParse(value.Slice(argSep + 1).Trim(), out argument))
                {
                    return false;
                }

                value = value.Slice(0, argSep).Trim();
            }

            return ParseEnum(SpellComponents, value, out component);
        }

        private static bool ParseSpellListEntry(ReadOnlySpan<char> value, out SpellEntryLevelSpec spellListEntry)
        {
            var sep = value.IndexOf(' ');
            if (sep == -1)
            {
                spellListEntry = default;
                return false;
            }

            var spellListName = value.Slice(0, sep).Trim();
            var spellListLevelText = value.Slice(sep + 1).Trim();

            if (!int.TryParse(spellListLevelText, out var spellLevel))
            {
                spellListEntry = default;
                return false;
            }

            if (ParseEnum(ClassSpellLists, spellListName, out var classEnum))
            {
                spellListEntry = new SpellEntryLevelSpec(classEnum, spellLevel);
                return true;
            }

            if (ParseEnum(DomainSpellLists, spellListName, out var domain))
            {
                spellListEntry = new SpellEntryLevelSpec(domain, spellLevel);
                return true;
            }

            spellListEntry = default;
            return false;
        }

        private static bool ParseEnum<T>(IEnumerable<(string, T)> constants,
            ReadOnlySpan<char> value,
            out T constantOut)
        {
            foreach (var (text, constant) in constants)
            {
                if (value.Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    constantOut = constant;
                    return true;
                }
            }

            constantOut = default;
            return false;
        }

        private static readonly (string, UiPickerType)[] PickerModes =
        {
            ("None", UiPickerType.None),
            ("Single", UiPickerType.Single),
            ("Multi", UiPickerType.Multi),
            ("Cone", UiPickerType.Cone),
            ("Area", UiPickerType.Area),
            ("Location", UiPickerType.Location),
            ("Personal", UiPickerType.Personal),
            ("Inventory", UiPickerType.InventoryItem),
            ("Ray", UiPickerType.Ray),
            ("Become_Touch", UiPickerType.BecomeTouch),
            ("Area or Obj", UiPickerType.AreaOrObj),
            ("Once-Multi", UiPickerType.OnceMulti),
            ("Any 30 Feet", UiPickerType.Any30Feet),
            ("Primary 30 Feet", UiPickerType.Primary30Feet),
            ("End Early Multi", UiPickerType.EndEarlyMulti),
            ("Loc Is Clear", UiPickerType.LocIsClear),
        };

        private static readonly (string, UiPickerIncFlags)[] TargetIncludeFlags =
        {
            ("None", UiPickerIncFlags.UIPI_None),
            ("Self", UiPickerIncFlags.UIPI_Self),
            ("Other", UiPickerIncFlags.UIPI_Other),
            ("Non-Critter", UiPickerIncFlags.UIPI_NonCritter),
            ("Dead", UiPickerIncFlags.UIPI_Dead),
            ("Undead", UiPickerIncFlags.UIPI_Undead),
            ("Unconscious", UiPickerIncFlags.UIPI_Unconscious),
            ("Hostile", UiPickerIncFlags.UIPI_Hostile),
            ("Friendly", UiPickerIncFlags.UIPI_Friendly),
            ("Potion", UiPickerIncFlags.UIPI_Potion),
            ("Scroll", UiPickerIncFlags.UIPI_Scroll),
        };

        private static readonly (string, UiPickerFlagsTarget)[] TargetFlags =
        {
            ("None", UiPickerFlagsTarget.None),
            ("Min", UiPickerFlagsTarget.Min),
            ("Max", UiPickerFlagsTarget.Max),
            ("Radius", UiPickerFlagsTarget.Radius),
            ("Range", UiPickerFlagsTarget.Range),
            ("Exclude_1st", UiPickerFlagsTarget.Exclude1st),
            ("Degrees", UiPickerFlagsTarget.Degrees),
            ("Fixed-radius", UiPickerFlagsTarget.FixedRadius),
        };

        private static readonly (string, SpellResistanceType)[] SpellResistanceTypes =
        {
            ("No", SpellResistanceType.No),
            ("Yes", SpellResistanceType.Yes),
            ("In-Code", SpellResistanceType.InCode)
        };

        private static readonly (string, bool)[] YesNo =
        {
            ("No", false),
            ("Yes", true)
        };

        private static readonly (string, SpellSavingThrow)[] SavingThrows =
        {
            ("None", SpellSavingThrow.None),
            ("Reflex", SpellSavingThrow.Reflex),
            ("Willpower", SpellSavingThrow.Willpower),
            ("Fortitude", SpellSavingThrow.Fortitude),
        };

        private static readonly (string, SpellRangeType)[] SpellRanges =
        {
            ("specified", SpellRangeType.SRT_Specified),
            ("personal", SpellRangeType.SRT_Personal),
            ("touch", SpellRangeType.SRT_Touch),
            ("close", SpellRangeType.SRT_Close),
            ("medium", SpellRangeType.SRT_Medium),
            ("long", SpellRangeType.SRT_Long),
            ("unlimited", SpellRangeType.SRT_Unlimited),
            ("special_invisibility_purge", SpellRangeType.SRT_Special_Inivibility_Purge),
        };

        private static readonly (string, SpellCastingTime)[] CastingTimes =
        {
            ("1 action", SpellCastingTime.StandardAction),
            ("Full Round", SpellCastingTime.FullRoundAction),
            ("Out of Combat", SpellCastingTime.OutOfCombat),
            ("Safe", SpellCastingTime.Safe),
            ("Free Action", SpellCastingTime.FreeAction),
        };

        private static readonly (string, SpellComponent)[] SpellComponents =
        {
            ("V", SpellComponent.Verbal),
            ("S", SpellComponent.Somatic),
            ("XP", SpellComponent.Experience),
            ("GP", SpellComponent.Material)
        };

        private static readonly (string, Stat)[] ClassSpellLists =
        {
            ("Barbarian", Stat.level_barbarian),
            ("Bbn", Stat.level_barbarian),
            ("Bard", Stat.level_bard),
            ("Brd", Stat.level_bard),
            ("Cleric", Stat.level_cleric),
            ("Clr", Stat.level_cleric),
            ("Druid", Stat.level_druid),
            ("Drd", Stat.level_druid),
            ("Fighter", Stat.level_fighter),
            ("Ftr", Stat.level_fighter),
            ("Mnk", Stat.level_monk),
            ("Pal", Stat.level_paladin),
            ("Ranger", Stat.level_ranger),
            ("Rgr", Stat.level_ranger),
            ("Rogue", Stat.level_rogue),
            ("Rog", Stat.level_rogue),
            ("Sorcerer", Stat.level_sorcerer),
            ("Sor", Stat.level_sorcerer),
            ("Wizard", Stat.level_wizard),
            ("Wiz", Stat.level_wizard),
        };

        private static readonly (string, DomainId)[] DomainSpellLists =
        {
            ("None", DomainId.None),
            ("Air", DomainId.Air),
            ("Animal", DomainId.Animal),
            ("Chaos", DomainId.Chaos),
            ("Death", DomainId.Death),
            ("Destruction", DomainId.Destruction),
            ("Earth", DomainId.Earth),
            ("Evil", DomainId.Evil),
            ("Fire", DomainId.Fire),
            ("Good", DomainId.Good),
            ("Healing", DomainId.Healing),
            ("Knowledge", DomainId.Knowledge),
            ("Law", DomainId.Law),
            ("Luck", DomainId.Luck),
            ("Magic", DomainId.Magic),
            ("Plant", DomainId.Plant),
            ("Protection", DomainId.Protection),
            ("Strength", DomainId.Strength),
            ("Sun", DomainId.Sun),
            ("Travel", DomainId.Travel),
            ("Trickery", DomainId.Trickery),
            ("War", DomainId.War),
            ("Water", DomainId.Water),
            ("Special", DomainId.Special)
        };

        private static readonly (string, AiSpellType)[] AiSpellTypes =
        {
            ("ai_action_summon", AiSpellType.ai_action_summon),
            ("ai_action_offensive", AiSpellType.ai_action_offensive),
            ("ai_action_defensive", AiSpellType.ai_action_defensive),
            ("ai_action_flee", AiSpellType.ai_action_flee),
            ("ai_action_heal_heavy", AiSpellType.ai_action_heal_heavy),
            ("ai_action_heal_medium", AiSpellType.ai_action_heal_medium),
            ("ai_action_heal_light", AiSpellType.ai_action_heal_light),
            ("ai_action_cure_poison", AiSpellType.ai_action_cure_poison),
            ("ai_action_resurrect", AiSpellType.ai_action_resurrect),
        };

        private static readonly (string, SchoolOfMagic)[] Schools =
        {
            ("None", SchoolOfMagic.None),
            ("Abjuration", SchoolOfMagic.Abjuration),
            ("Conjuration", SchoolOfMagic.Conjuration),
            ("Divination", SchoolOfMagic.Divination),
            ("Enchantment", SchoolOfMagic.Enchantment),
            ("Evocation", SchoolOfMagic.Evocation),
            ("Illusion", SchoolOfMagic.Illusion),
            ("Necromancy", SchoolOfMagic.Necromancy),
            ("Transmutation", SchoolOfMagic.Transmutation),
        };

        private static readonly (string, SubschoolOfMagic)[] SubSchools =
        {
            ("Calling", SubschoolOfMagic.Calling),
            ("Creation", SubschoolOfMagic.Creation),
            ("Healing", SubschoolOfMagic.Healing),
            ("Summoning", SubschoolOfMagic.Summoning),
            ("Charm", SubschoolOfMagic.Charm),
            ("Compulsion", SubschoolOfMagic.Compulsion),
            ("Figment", SubschoolOfMagic.Figment),
            ("Glamer", SubschoolOfMagic.Glamer),
            ("Pattern", SubschoolOfMagic.Pattern),
            ("Phantasm", SubschoolOfMagic.Phantasm),
            ("Shadow", SubschoolOfMagic.Shadow),
            ("Scrying", SubschoolOfMagic.Scrying)
        };

        private static readonly (string, SpellDescriptor)[] Descriptors =
        {
            ("ACID", SpellDescriptor.ACID),
            ("CHAOTIC", SpellDescriptor.CHAOTIC),
            ("COLD", SpellDescriptor.COLD),
            ("DARKNESS", SpellDescriptor.DARKNESS),
            ("DEATH", SpellDescriptor.DEATH),
            ("ELECTRICITY", SpellDescriptor.ELECTRICITY),
            ("EVIL", SpellDescriptor.EVIL),
            ("FEAR", SpellDescriptor.FEAR),
            ("FIRE", SpellDescriptor.FIRE),
            ("FORCE", SpellDescriptor.FORCE),
            ("GOOD", SpellDescriptor.GOOD),
            // ToEE only did prefix-comparisons and used the prefix "LANGUAGE" here...
            ("LANGUAGE", SpellDescriptor.LANGUAGE_DEPENDENT),
            ("Language-Dependent", SpellDescriptor.LANGUAGE_DEPENDENT),
            ("LANGUAGE_DEPENDENT", SpellDescriptor.LANGUAGE_DEPENDENT),
            ("LAWFUL", SpellDescriptor.LAWFUL),
            ("LIGHT", SpellDescriptor.LIGHT),
            // ToEE only did prefix-comparisons and used the prefix "MIND" here...
            ("MIND", SpellDescriptor.MIND_AFFECTING),
            ("MIND-AFFECTING", SpellDescriptor.MIND_AFFECTING),
            ("SONIC", SpellDescriptor.SONIC),
            ("TELEPORTATION", SpellDescriptor.TELEPORTATION),
            ("AIR", SpellDescriptor.AIR),
            ("EARTH", SpellDescriptor.EARTH),
            ("WATER", SpellDescriptor.WATER),
        };

        [TempleDllLocation(0x1007a890)]
        private static bool ParseSpellLine(string line, out SpellLineKey key, out ReadOnlySpan<char> value)
        {
            ReadOnlySpan<char> text = line;
            var sep = text.IndexOf(':');
            if (sep == -1)
            {
                key = default;
                value = ReadOnlySpan<char>.Empty;
                return false;
            }

            Span<char> lowerCaseKey = stackalloc char[sep];
            text.Slice(0, sep).CopyTo(lowerCaseKey);
            for (var i = 0; i < lowerCaseKey.Length; i++)
            {
                lowerCaseKey[i] = char.ToLowerInvariant(text[i]);
            }

            value = text.Slice(sep + 1).Trim();
            ReadOnlySpan<char> lowerCaseKeyReadOnly = lowerCaseKey;

            if (lowerCaseKeyReadOnly.Equals("school", StringComparison.Ordinal))
            {
                key = SpellLineKey.School;
            }
            else if (lowerCaseKeyReadOnly.Equals("subschool", StringComparison.Ordinal)
                     // subschools is a typo that ToEE accepted due to only checking for prefix
                || lowerCaseKeyReadOnly.Equals("subschools", StringComparison.Ordinal))
            {
                key = SpellLineKey.Subschool;
            }
            else if (lowerCaseKeyReadOnly.Equals("descriptor", StringComparison.Ordinal))
            {
                key = SpellLineKey.Descriptor;
            }
            else if (lowerCaseKeyReadOnly.Equals("level", StringComparison.Ordinal))
            {
                key = SpellLineKey.Level;
            }
            else if (lowerCaseKeyReadOnly.Equals("component", StringComparison.Ordinal))
            {
                key = SpellLineKey.Component;
            }
            else if (lowerCaseKeyReadOnly.Equals("casting time", StringComparison.Ordinal))
            {
                key = SpellLineKey.CastingTime;
            }
            else if (lowerCaseKeyReadOnly.Equals("range", StringComparison.Ordinal))
            {
                key = SpellLineKey.Range;
            }
            else if (lowerCaseKeyReadOnly.Equals("saving throw", StringComparison.Ordinal))
            {
                key = SpellLineKey.SavingThrow;
            }
            else if (lowerCaseKeyReadOnly.Equals("spell resistance", StringComparison.Ordinal))
            {
                key = SpellLineKey.SpellResistance;
            }
            else if (lowerCaseKeyReadOnly.Equals("projectile", StringComparison.Ordinal))
            {
                key = SpellLineKey.Projectile;
            }
            else if (lowerCaseKeyReadOnly.Equals("flags_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.FlagsTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("inc_flags_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.IncFlagsTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("exc_flags_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.ExcFlagsTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("mode_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.ModeTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("min_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.MinTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("max_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.MaxTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("radius_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.RadiusTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("degrees_target", StringComparison.Ordinal))
            {
                key = SpellLineKey.DegreesTarget;
            }
            else if (lowerCaseKeyReadOnly.Equals("ai_type", StringComparison.Ordinal))
            {
                key = SpellLineKey.AiType;
            }
            else
            {
                key = default;
                return false;
            }

            return true;
        }

        private enum SpellLineKey
        {
            School = 0,
            Subschool = 1,
            Descriptor = 2,
            Level = 3,
            Component = 4,
            CastingTime = 5,
            Range = 6,
            SavingThrow = 7,
            SpellResistance = 8,
            Projectile = 9,
            FlagsTarget = 10,
            IncFlagsTarget = 11,
            ExcFlagsTarget = 12,
            ModeTarget = 13,
            MinTarget = 14,
            MaxTarget = 15,
            RadiusTarget = 16,
            DegreesTarget = 17,
            AiType = 18
        }
    }
}