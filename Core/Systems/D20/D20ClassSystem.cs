using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Feats;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20ClassSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public static readonly Stat[] VanillaClasses =
        {
            Stat.level_barbarian, Stat.level_bard, Stat.level_cleric,
            Stat.level_druid, Stat.level_fighter, Stat.level_monk,
            Stat.level_paladin, Stat.level_ranger, Stat.level_rogue,
            Stat.level_sorcerer, Stat.level_wizard
        };

        public static readonly Stat[] AllClasses =
        {
            Stat.level_barbarian, Stat.level_bard, Stat.level_cleric,
            Stat.level_druid, Stat.level_fighter, Stat.level_monk,
            Stat.level_paladin, Stat.level_ranger, Stat.level_rogue,
            Stat.level_sorcerer, Stat.level_wizard
        };

        private static readonly Dictionary<Stat, D20ClassSpec> classSpecs = new Dictionary<Stat, D20ClassSpec>();

        [TempleDllLocation(0x1007a3f0)]
        public static bool IsCastingClass(Stat classId, bool includeExtenders = false)
        {
            if (!classSpecs.TryGetValue(classId, out var classSpec))
                return false;

            if (includeExtenders && classSpec.spellListType == SpellListType.Extender)
                return true;

            else if (classSpec.spellListType == SpellListType.None)
            {
                return false;
            }

            return true;
        }

        public static int GetBaseAttackBonus(Stat classId, int levels)
        {
            if (!classSpecs.TryGetValue(classId, out var classSpec))
                return 0;

            var babProg = classSpec.babProgression;

            switch (babProg)
            {
                case BABProgressionType.Martial:
                    return levels;
                case BABProgressionType.SemiMartial:
                    return (3 * levels) / 4;
                case BABProgressionType.NonMartial:
                    return levels / 2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(babProg), babProg, "Unknown BAB progression");
            }
        }

        public static bool HasFeat(FeatId featId, Stat classId, int classLevels)
        {
            if (!classSpecs.TryGetValue(classId, out var classSpec))
                return false;

            if (!classSpec.classFeats.TryGetValue(featId, out var minLevels))
            {
                return false;
            }

            return classLevels >= minLevels;
        }

        public static int GetClassHitDice(Stat classId)
        {
            if (classSpecs.TryGetValue(classId, out var classSpec))
            {
                return classSpec.hitDice;
            }
            else
            {
                Logger.Warn("Missing classSpec for {0}", classId);
                return 6;
            }
        }

        public static bool IsClassSkill(SkillId skillId, Stat levClass)
        {
            if (classSpecs.TryGetValue(levClass, out var classSpec))
            {
                if (classSpec.classSkills.TryGetValue(skillId, out var isClassSkill))
                {
                    return isClassSkill;
                }

                return false;
            }

            Logger.Warn("Missing classSpec for {0}", levClass);
            return false;
        }
    }
}