using System;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems
{
    public class LevelSystem : IGameSystem
    {
        //  xp required to reach a certain level, starting from level 0 (will be 0,0,1000,3000,6000,...)
        [TempleDllLocation(0x102AAF00)]
        private readonly int[] _xpTable = BuildXpTable();

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100731e0)]
        public int LevelUpApply(GameObjectBody obj, LevelupPacket levelUpPacket)
        {
            var numLvls = obj.GetArrayLength(obj_f.critter_level_idx);
            obj.SetInt32(obj_f.critter_level_idx, numLvls, (int) levelUpPacket.classCode);

            if ((levelUpPacket.flags & 1) == 0)
            {
                // Raise stat
                var scoreRaised = levelUpPacket.abilityScoreRaised;
                if (scoreRaised >= Stat.strength && scoreRaised <= Stat.charisma)
                {
                    var currentValue = obj.GetBaseStat(levelUpPacket.abilityScoreRaised);
                    GameSystems.Stat.SetBasicStat(obj, levelUpPacket.abilityScoreRaised, currentValue + 1);
                }

                foreach (var feat in levelUpPacket.feats)
                {
                    if (feat != FeatId.NONE)
                    {
                        GameSystems.Feat.AddFeat(obj, feat);
                    }

                    if (feat == FeatId.SKILL_MASTERY)
                    {
                        GameUiBridge.ApplySkillMastery(obj);
                    }
                }

                foreach (var kvp in levelUpPacket.skillPointsAdded)
                {
                    GameSystems.Skill.AddSkillRanks(obj, kvp.Key, kvp.Value);
                }

                var spellClassCode = ((int) levelUpPacket.classCode & 0x7F) | 0x80;

                var spellToRemove = levelUpPacket.spellEnumToRemove;
                if (spellToRemove != 0)
                {
                    var spellLevel = GameSystems.Spell.GetSpellLevelBySpellClass(spellToRemove, spellClassCode);
                    GameSystems.Spell.SpellKnownRemove(obj, levelUpPacket.spellEnumToRemove, spellLevel,
                        spellClassCode);
                }

                foreach (var spellEnum in levelUpPacket.spellEnums)
                {
                    var spellLevel = GameSystems.Spell.GetSpellLevelBySpellClass(spellEnum, spellClassCode);
                    GameSystems.Spell.SpellKnownAdd(obj, spellEnum, spellClassCode, spellLevel, 1, 0);
                }

                var conMod = obj.GetBaseStat(Stat.con_mod);
                var classHitDie = D20ClassSystem.GetClassHitDice(levelUpPacket.classCode);

                var hpRolled = classHitDie.Roll();
                if (hpRolled + conMod < 1)
                {
                    hpRolled = 1 - conMod;
                }

                var maxHp = obj.GetBaseStat(Stat.hp_max);
                GameSystems.Stat.SetBasicStat(obj, Stat.hp_max, maxHp + hpRolled);

                GameSystems.D20.Status.D20StatusRefresh(obj);
                GameSystems.Critter.BuildRadialMenu(obj);
            }

            return obj.GetBaseStat(levelUpPacket.classCode);
        }

        /// <summary>
        /// Returns the amount of experience needed to reach a given character level.
        /// </summary>
        [TempleDllLocation(0x100802e0)]
        public int GetExperienceForLevel(int level)
        {
            if (level < 0 || level >= _xpTable.Length)
            {
                return int.MaxValue;
            }

            return _xpTable[level];
        }

        /// <summary>
        /// Returns the next level that'll be reached given the current experience points.
        /// </summary>
        [TempleDllLocation(0x10080300)]
        public int GetNextLevelForXp(int experience)
        {
            if (experience > _xpTable[21])
            {
                return 21;
            }
            else
            {
                // Find the current level given the XP
                var currentLevel = 20;
                while (experience < _xpTable[currentLevel + 1])
                {
                    if (--currentLevel < 0)
                    {
                        return 0;
                    }
                }

                return currentLevel + 1;
            }
        }

        private static int[] BuildXpTable()
        {
            var result = new int[100];

            result[2] = 1000;
            for (var i = 3; i < result.Length; i++)
            {
                result[i] = 1000 * (i - 1) * i / 2;
            }

            return result;
        }

        [TempleDllLocation(0x10073420)]
        public void NpcAddKnownSpells(GameObjectBody obj)
        {
            if (!obj.IsNPC())
            {
                return;
            }

            // TODO Replace this with info from D20ClassSystem
            Stat[] memorizingClasses = {
                Stat.level_cleric,
                Stat.level_druid,
                Stat.level_paladin,
                Stat.level_ranger,
                Stat.level_wizard
            };

            foreach (var castingClass in memorizingClasses)
            {
                var levels = obj.GetStat(castingClass);
                if (levels == 0)
                {
                    continue;
                }

                var castingLevel = GameSystems.Spell.GetMaxSpellLevel(obj, castingClass, levels);
                if (castingClass == Stat.level_wizard)
                {
                    castingLevel = 0;
                }

                foreach (var spellEntry in GameSystems.Spell.EnumerateLearnableSpells(obj))
                {
                    foreach (var spellLevel in spellEntry.spellLvls)
                    {
                        if (!GameSystems.Spell.IsDomainSpell(spellLevel.spellClass)
                            && GameSystems.Spell.GetCastingClass(spellLevel.spellClass) == castingClass
                            && spellLevel.slotLevel <= castingLevel)
                        {
                            GameSystems.Spell.SpellKnownAdd(obj, spellEntry.spellEnum, spellLevel.spellClass,
                                spellLevel.slotLevel, 1, 0);
                        }

                        if (castingClass == Stat.level_cleric)
                        {
                            var domain1 = obj.GetStat(Stat.domain_1);
                            var domain2 = obj.GetStat(Stat.domain_2);

                            if ((spellLevel.spellClass & 0x80) == 0
                                && ((spellLevel.spellClass & 0x7F) == domain1
                                    || (spellLevel.spellClass & 0x7F) == domain2)
                                && spellLevel.slotLevel <= castingLevel)
                            {
                                GameSystems.Spell.SpellKnownAdd(obj, spellEntry.spellEnum, spellLevel.spellClass,
                                    spellLevel.slotLevel, 1, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the half-way-point between the current level and previous level.
        /// </summary>
        [TempleDllLocation(0x10077500)]
        private int GetXpAfterRaiseDead(int currentLevel)
        {
            Trace.Assert(currentLevel >= 1);
            var prevLevelXp = _xpTable[currentLevel - 1];
            var currentLevelXp = _xpTable[currentLevel];
            return (currentLevelXp + prevLevelXp) / 2;
        }

    }
}