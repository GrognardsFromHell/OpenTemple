using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;

namespace OpenTemple.Core.Systems
{
    public static class Resurrection
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static int GetLevelForResurrection(GameObject critter)
        {
            if (critter.IsPC())
            {
                return critter.GetStat(Stat.level);
            }
            else
            {
                return critter.GetArrayLength(obj_f.npc_hitdice_idx) + critter.GetStat(Stat.level);
            }
        }

        [TempleDllLocation(0x100809c0)]
        public static bool Resurrect(GameObject critter, ResurrectionType type)
        {
            var success = false;
            if (CanResurrect(critter, type))
            {
                ResurrectAndApplyPenalties(critter, type);
                success = true;
            }

            GameSystems.D20.D20SendSignal(critter, D20DispatcherKey.SIG_Resurrection);
            return success;
        }

        [TempleDllLocation(0x10080870)]
        public static bool CanResurrect(GameObject critter, ResurrectionType type)
        {
            if (!critter.HasFlag(ObjectFlag.DESTROYED) && GameSystems.Stat.GetCurrentHP(critter) > -10)
            {
                // Critter is not actually dead
                return false;
            }

            switch (type)
            {
                case ResurrectionType.RaiseDead:
                    var level = GetLevelForResurrection(critter);
                    if (level < 1 || level == 1 && critter.GetStat(Stat.constitution) <= 2)
                    {
                        return false;
                    }

                    if (critter.HasCondition(StatusEffects.KilledByDeathEffect))
                    {
                        return false;
                    }

                    switch (GameSystems.Critter.GetCategory(critter))
                    {
                        case MonsterCategory.construct:
                        case MonsterCategory.elemental:
                        case MonsterCategory.outsider:
                        // TODO: Fix that Tiefling should be able to be raised
                        case MonsterCategory.undead:
                            if (critter.ProtoId == WellKnownProtos.DarleyDemon
                                || critter.ProtoId == WellKnownProtos.DarleySorceress)
                            {
                                break;
                            }

                            return false;
                    }

                    return true;
                case ResurrectionType.CuthbertResurrection:
                    return true;
                // TODO: Missing resurrection and true resurrection
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resurrection type.");
            }
        }

        [TempleDllLocation(0x1007fd30)]
        private static bool ResurrectAndApplyPenalties(GameObject critter, ResurrectionType type)
        {
            switch (type)
            {
                case ResurrectionType.RaiseDead:
                    var level = GetLevelForResurrection(critter);
                    if (level == 1)
                    {
                        // Apply the constitution penalty for first level chars who cannot lose a level
                        var conScore = critter.GetBaseStat(Stat.constitution) - 2;
                        GameSystems.Stat.SetBasicStat(critter, Stat.constitution, conScore);
                    }
                    else if (critter.GetStat(Stat.level) > 0)
                    {
                        // TODO: Take into account adjustments for racial level adjustment
                        // Otherwise apply the XP reset for losing a level
                        var actualLevel = critter.GetStat(Stat.level); // Make sure no HD are included
                        var xpForCurrentLevel = GameSystems.Level.GetExperienceForLevel(actualLevel);
                        var xpForNewLevel = GameSystems.Level.GetExperienceForLevel(actualLevel - 1);
                        var newXp = (xpForCurrentLevel + xpForNewLevel) / 2;
                        critter.SetInt32(obj_f.critter_experience, newXp);
                    }

                    // raise at a number of hitpoints equal to the hit die/level
                    var maxHp = critter.GetStat(Stat.hp_max);
                    GameSystems.MapObject.ChangeTotalDamage(critter, maxHp - level);

                    // raise any attribute that has been lowered to 0 back up to 1.
                    for (var attribute = Stat.strength; attribute <= Stat.charisma; attribute++)
                    {
                        if (critter.GetStat(attribute) <= 0)
                        {
                            GameSystems.Stat.SetBasicStat(critter, attribute, 1);
                        }
                    }

                    GameSystems.Spell.FloatSpellLine(critter, 20037, TextFloaterColor.White);
                    GameSystems.Anim.PushAnimate(critter, NormalAnimType.Getup);
                    return true;
                case ResurrectionType.CuthbertResurrection:
                    GameSystems.MapObject.ChangeTotalDamage(critter, 0);
                    GameSystems.Spell.FloatSpellLine(critter, 20037, TextFloaterColor.White);
                    GameSystems.Anim.PushAnimate(critter, NormalAnimType.Getup);
                    return true;
                case ResurrectionType.Resurrection:
                case ResurrectionType.TrueResurrection:
                    // TODO: These are not implemented
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown resurrection type.");
            }
        }
    }

    public enum ResurrectionType
    {
        RaiseDead = 0,
        Resurrection = 1,
        TrueResurrection = 2,
        CuthbertResurrection = 3
    }
}
