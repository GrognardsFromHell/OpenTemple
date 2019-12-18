using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Classes
{
    public static class D20ClassSystem
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

        public static Stat[] ClassesWithSpellLists { get; }

        public static IImmutableDictionary<Stat, D20ClassSpec> Classes { get; }

        static D20ClassSystem()
        {
            var builder = ImmutableDictionary.CreateBuilder<Stat, D20ClassSpec>();

            foreach (var classSpec in ContentDiscovery.Classes)
            {
                builder.Add(classSpec.classEnum, classSpec);
            }

            Classes = builder.ToImmutable();

            ClassesWithSpellLists = Classes.Keys.Where(HasSpellList).ToArray();
        }

        [TempleDllLocation(0x1007a3f0)]
        public static bool IsCastingClass(Stat classId, bool includeExtenders = false)
        {
            if (!Classes.TryGetValue(classId, out var classSpec))
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
            if (!Classes.TryGetValue(classId, out var classSpec))
                return 0;

            var babProg = classSpec.BaseAttackBonusProgression;

            switch (babProg)
            {
                case BaseAttackProgressionType.Martial:
                    return levels;
                case BaseAttackProgressionType.SemiMartial:
                    return (3 * levels) / 4;
                case BaseAttackProgressionType.NonMartial:
                    return levels / 2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(babProg), babProg, "Unknown BAB progression");
            }
        }

        public static SpellListType GetSpellListType(Stat classId)
        {
            if (!Classes.TryGetValue(classId, out var classSpec))
            {
                return SpellListType.None;
            }

            return classSpec.spellListType;
        }

        public static bool HasSpellList(Stat classId)
        {
            if (!Classes.TryGetValue(classId, out var classSpec))
            {
                return false;
            }

            switch (classSpec.spellListType)
            {
                case SpellListType.None:
                    return false;
                case SpellListType.Arcane:
                case SpellListType.Bardic:
                case SpellListType.Clerical:
                case SpellListType.Druidic:
                case SpellListType.Paladin:
                case SpellListType.Ranger:
                case SpellListType.Special:
                    return true;
                default:
                    return false;
            }
        }

        public static bool HasFeat(FeatId featId, Stat classId, int classLevels)
        {
            if (!Classes.TryGetValue(classId, out var classSpec))
                return false;

            if (!classSpec.classFeats.TryGetValue(featId, out var minLevels))
            {
                return false;
            }

            return classLevels >= minLevels;
        }

        public static Dice GetClassHitDice(Stat classId)
        {
            if (Classes.TryGetValue(classId, out var classSpec))
            {
                return new Dice(1, classSpec.hitDice);
            }
            else
            {
                Logger.Warn("Missing classSpec for {0}", classId);
                return Dice.D6;
            }
        }

        public static bool IsClassSkill(SkillId skillId, Stat levClass)
        {
            if (Classes.TryGetValue(levClass, out var classSpec))
            {
                return classSpec.classSkills.Contains(skillId);
            }

            Logger.Warn("Missing classSpec for {0}", levClass);
            return false;
        }

        public static bool IsNaturalCastingClass(Stat classCode)
        {
            if (!Classes.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            return classSpec.spellMemorizationType == SpellReadyingType.Innate;
        }

        public static Stat GetSpellStat(Stat classCode)
        {
            if (!Classes.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            return classSpec.spellStat;
        }

        public static bool IsArcaneCastingClass(Stat classCode)
        {
            if (!Classes.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            if (classSpec.spellListType == SpellListType.None)
            {
                return false;
            }

            if (classSpec.spellSourceType == SpellSourceType.Arcane)
            {
                return true;
            }

            return false;
        }

        public static bool IsDivineCastingClass(Stat classCode)
        {
            if (!Classes.TryGetValue(classCode, out var classSpec))
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
            if (!Classes.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            return classSpec.spellMemorizationType == SpellReadyingType.Vancian;
        }

        public static Stat GetSpellDcStat(Stat classCode)
        {
            if (!Classes.TryGetValue(classCode, out var classSpec))
            {
                throw new ArgumentException("Unknown class: " + classCode);
            }

            if (classSpec.spellDcStat == Stat.strength)
            {
                return classSpec.spellStat;
            }

            return classSpec.spellDcStat;
        }

        [TempleDllLocation(0x100f5660)]
        public static int GetNumSpellsFromClass(GameObjectBody caster, Stat classCode, int spellLvl, int classLvl,
            bool getFromStatMod = true)
        {
            var result = -1;

            if (!Classes.TryGetValue(classCode, out var classSpec))
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

            if (getFromStatMod)
            {
                result += GetBonusSpells(caster, classCode, spellLvl);
            }

            return result;
        }

        public static int GetSpecialisationSlots(GameObjectBody caster, Stat castingClass, int spellLevel)
        {
            if (castingClass == Stat.level_wizard
                && GameSystems.Spell.GetSchoolSpecialization(caster, out _, out _, out _)
                && spellLevel >= 1)
            {
                return 1;
            }

            return 0;
        }

        public static int GetBonusSpells(GameObjectBody caster, Stat castingClass, int spellLevel)
        {
            if (spellLevel == 0)
            {
                return 0; // No bonus spells for cantrips
            }

            var spellStat = GetSpellStat(castingClass);
            var spellStatMod = D20StatSystem.GetModifierForAbilityScore(caster.GetStat(spellStat));
            if (spellStatMod >= spellLevel)
            {
                return ((spellStatMod - spellLevel) / 4) + 1;
            }
            else
            {
                return 0;
            }
        }

        public static Stat GetHighestArcaneCastingClass(GameObjectBody critter)
        {
            var highestClass = (Stat) 0;
            var highestLvl = 0;

            foreach (var classEnum in ClassesWithSpellLists)
            {
                if (IsArcaneCastingClass(classEnum))
                {
                    var lvlThis = critter.GetStat(classEnum);
                    if (lvlThis > highestLvl)
                    {
                        highestLvl = lvlThis;
                        highestClass = classEnum;
                    }
                }
            }

            return highestClass;
        }

        public static Stat GetHighestDivineCastingClass(GameObjectBody critter)
        {
            var highestClass = (Stat) 0;
            var highestLvl = 0;

            foreach (var classEnum in ClassesWithSpellLists)
            {
                if (IsDivineCastingClass(classEnum))
                {
                    var lvlThis = critter.GetStat(classEnum);
                    if (lvlThis > highestLvl)
                    {
                        highestLvl = lvlThis;
                        highestClass = classEnum;
                    }
                }
            }

            return highestClass;
        }
    }
}