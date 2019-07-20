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

        public static bool IsNaturalCastingClass(Stat classCode)
        {
            if (!classSpecs.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            return classSpec.spellMemorizationType == SpellReadyingType.Innate;
        }

        public static Stat GetSpellStat(Stat classCode)
        {
            if (!classSpecs.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            return classSpec.spellStat;
        }

        public static bool IsDivineCastingClass(Stat classCode)
        {
            if (!classSpecs.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            if (classSpec.spellListType == SpellListType.None)
            {
                return false;
            }

            if (classSpec.spellSourceType == SpellSourceType.Divine)
            {
                return true;
            }

            return false;
        }

        public static bool IsVancianCastingClass(Stat classCode)
        {
            if (!classSpecs.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            return classSpec.spellMemorizationType == SpellReadyingType.Vancian;
        }

        public static Stat GetSpellDcStat(Stat classCode)
        {
            if (!classSpecs.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            if (classSpec.spellDcStat == Stat.strength)
            {
                return classSpec.spellStat;
            }

            return classSpec.spellDcStat;
        }

        public static int GetNumSpellsFromClass(GameObjectBody caster, Stat classCode, int spellLvl, int classLvl,
            bool getFromStatMod = true)
        {
            var result = -1;

            if (!classSpecs.TryGetValue(classCode, out var classSpec))
            {
                return -1;
            }

            var spellsPerDay = classSpec.spellsPerDay;

            // if not found, get highest specified
            if (!spellsPerDay.TryGetValue(classLvl, out var spellsPerDayForLvl))
            {
                var highestSpec = -1;
                foreach (var it in spellsPerDay.Keys)
                {
                    if (it > highestSpec && it <= classLvl)
                        highestSpec = it;
                }

                if (highestSpec == -1)
                    return -1;
                spellsPerDayForLvl = spellsPerDay[highestSpec];
            }

            if (spellsPerDayForLvl.Count < spellLvl + 1)
                return -1;
            if (spellsPerDayForLvl[spellLvl] < 0)
                return -1;

            result = spellsPerDayForLvl[spellLvl];

            if (!getFromStatMod || spellLvl == 0)
                return result;

            var spellStat = GetSpellStat(classCode);
            var spellStatMod = D20StatSystem.GetModifierForAbilityScore(caster.GetStat(spellStat));
            if (spellStatMod >= spellLvl)
                result += ((spellStatMod - spellLvl) / 4) + 1;

            return result;
        }
    }
}