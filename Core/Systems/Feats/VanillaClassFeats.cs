using System.Collections.Generic;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.Feats;

public class VanillaClassFeats
{
    private static readonly Dictionary<Stat, FeatEntry[]> ClassEntries = new Dictionary<Stat, FeatEntry[]>();

    static VanillaClassFeats()
    {
        ClassEntries[Stat.level_barbarian] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_MEDIUM, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY, 1),
            new FeatEntry(FeatId.BARBARIAN_RAGE, 1),
            new FeatEntry(FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1),
            new FeatEntry(FeatId.UNCANNY_DODGE, 2),
            new FeatEntry(FeatId.FAST_MOVEMENT, 1),
            new FeatEntry(FeatId.IMPROVED_UNCANNY_DODGE, 5),
            new FeatEntry(FeatId.GREATER_RAGE, 11),
            new FeatEntry(FeatId.INDOMITABLE_WILL, 14),
            new FeatEntry(FeatId.TIRELESS_RAGE, 17),
            new FeatEntry(FeatId.MIGHTY_RAGE, 20),
        };

        ClassEntries[Stat.level_bard] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY_BARD, 1),
            new FeatEntry(FeatId.BARDIC_MUSIC, 1),
            new FeatEntry(FeatId.BARDIC_KNOWLEDGE, 1),
        };

        ClassEntries[Stat.level_cleric] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_MEDIUM, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_HEAVY, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY, 1),
            new FeatEntry(FeatId.DOMAIN_POWER, 1),
        };

        ClassEntries[Stat.level_druid] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.ANIMAL_COMPANION, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY_DRUID, 1),
            new FeatEntry(FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSPEAR, 1),
            new FeatEntry(FeatId.MARTIAL_WEAPON_PROFICIENCY_SCIMITAR, 1),
            new FeatEntry(FeatId.NATURE_SENSE, 1),
            new FeatEntry(FeatId.WOODLAND_STRIDE, 2),
            new FeatEntry(FeatId.TRACKLESS_STEP, 3),
            new FeatEntry(FeatId.RESIST_NATURES_LURE, 4),
            new FeatEntry(FeatId.WILD_SHAPE, 5),
            new FeatEntry(FeatId.VENOM_IMMUNITY, 9),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_MEDIUM, 1),
        };

        ClassEntries[Stat.level_fighter] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_MEDIUM, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_HEAVY, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY, 1),
            new FeatEntry(FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1),
        };

        ClassEntries[Stat.level_monk] = new[]
        {
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY_MONK, 1),
            new FeatEntry(FeatId.IMPROVED_UNARMED_STRIKE, 1),
            new FeatEntry(FeatId.STUNNING_ATTACKS, 1),
            new FeatEntry(FeatId.STUNNING_FIST, 1),
            new FeatEntry(FeatId.WHOLENESS_OF_BODY, 7),
            new FeatEntry(FeatId.FAST_MOVEMENT, 3),
            new FeatEntry(FeatId.FLURRY_OF_BLOWS, 1),
            new FeatEntry(FeatId.EVASION, 2),
            new FeatEntry(FeatId.STILL_MIND, 3),
            new FeatEntry(FeatId.PURITY_OF_BODY, 5),
            new FeatEntry(FeatId.IMPROVED_TRIP, 6),
            new FeatEntry(FeatId.IMPROVED_EVASION, 9),
            new FeatEntry(FeatId.KI_STRIKE, 4),
            new FeatEntry(FeatId.MONK_DIAMOND_BODY, 11),
            new FeatEntry(FeatId.MONK_ABUNDANT_STEP, 12),
            new FeatEntry(FeatId.MONK_DIAMOND_SOUL, 13),
            new FeatEntry(FeatId.MONK_QUIVERING_PALM, 15),
            new FeatEntry(FeatId.MONK_EMPTY_BODY, 19),
            new FeatEntry(FeatId.MONK_PERFECT_SELF, 20),
        };

        ClassEntries[Stat.level_paladin] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_MEDIUM, 1),
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_HEAVY, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY, 1),
            new FeatEntry(FeatId.LAY_ON_HANDS, 2),
            new FeatEntry(FeatId.SMITE_EVIL, 1),
            new FeatEntry(FeatId.REMOVE_DISEASE, 6),
            new FeatEntry(FeatId.TURN_UNDEAD, 4),
            new FeatEntry(FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1),
            new FeatEntry(FeatId.DETECT_EVIL, 1),
            new FeatEntry(FeatId.AURA_OF_COURAGE, 3),
            new FeatEntry(FeatId.DIVINE_HEALTH, 3),
            new FeatEntry(FeatId.DIVINE_GRACE, 2),
            new FeatEntry(FeatId.SPECIAL_MOUNT, 5),
            new FeatEntry(FeatId.CODE_OF_CONDUCT, 1),
            new FeatEntry(FeatId.ASSOCIATES, 1),
        };

        ClassEntries[Stat.level_ranger] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.SHIELD_PROFICIENCY, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY, 1),
            new FeatEntry(FeatId.TRACK, 1),
            new FeatEntry(FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1),
            new FeatEntry(FeatId.ANIMAL_COMPANION, 4),
            new FeatEntry(FeatId.EVASION, 9),
        };

        ClassEntries[Stat.level_rogue] = new[]
        {
            new FeatEntry(FeatId.ARMOR_PROFICIENCY_LIGHT, 1),
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY_ROGUE, 1),
            new FeatEntry(FeatId.UNCANNY_DODGE, 4),
            new FeatEntry(FeatId.SNEAK_ATTACK, 1),
            new FeatEntry(FeatId.TRAPS, 1),
            new FeatEntry(FeatId.EVASION, 2),
            new FeatEntry(FeatId.IMPROVED_UNCANNY_DODGE, 8),
            new FeatEntry(FeatId.SKILL_MASTERY, 10),
        };

        ClassEntries[Stat.level_sorcerer] = new[]
        {
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY, 1),
            new FeatEntry(FeatId.CALL_FAMILIAR, 1),
        };

        ClassEntries[Stat.level_wizard] = new[]
        {
            new FeatEntry(FeatId.SIMPLE_WEAPON_PROFICIENCY_WIZARD, 1),
            new FeatEntry(FeatId.SCRIBE_SCROLL, 1),
            new FeatEntry(FeatId.CALL_FAMILIAR, 1),
        };
    }

    private readonly struct FeatEntry
    {
        public readonly FeatId Feat;

        public readonly int MinLevel;

        public FeatEntry(FeatId feat, int minLevel)
        {
            Feat = feat;
            MinLevel = minLevel;
        }
    }

    public static bool HasFeatImplicitly(FeatId featId, Stat classId, int classLevel)
    {
        if (ClassEntries.TryGetValue(classId, out var featEntries))
        {
            foreach (var entry in featEntries)
            {
                if (entry.Feat == featId && entry.MinLevel <= classLevel)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool HasRangerArcheryStyleFeatImplicitly(FeatId featId, int rangerLevels)
    {
        switch (featId)
        {
            case FeatId.RANGER_RAPID_SHOT:
                return rangerLevels >= 2;
            case FeatId.RANGER_MANYSHOT:
                return rangerLevels >= 6;
            default:
                return false;
        }
    }

    public static bool HasRangerTwoWeaponStyleFeatImplicitly(FeatId featId, int rangerLevels)
    {
        switch (featId)
        {
            case FeatId.TWO_WEAPON_FIGHTING_RANGER:
                return rangerLevels >= 2;
            case FeatId.IMPROVED_TWO_WEAPON_FIGHTING_RANGER:
                return rangerLevels >= 6;
            default:
                return false;
        }
    }

}