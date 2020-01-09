using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Script.Hooks;
using OpenTemple.Core.Systems.Spells;

namespace Scripts
{
    public class IgnoreTargetHook : IShouldIgnoreTargetHook
    {
        public bool ShouldIgnoreTarget(GameObjectBody npc, GameObjectBody target)
        {
            if (IsSpiritualWeapon(target))
            {
                return true;
            }

            if (target.HasFlag(ObjectFlag.DONTDRAW))
            {
                return true;
            }

            if (target.D20Query(D20DispatcherKey.QUE_Is_Ethereal))
            {
                return true;
            }

            var isIntelligent = npc.GetStat(Stat.intelligence) >= 3;
            if (!isIntelligent)
            {
                return false;
            }

            if (IsWarded(target))
            {
                return true;
            }

            if (target.HasCondition(SpellEffects.SpellSummoned))
            {
                var attacheePowerLvl = GetPowerLevel(npc);
                var targetPowerLvl = GetPowerLevel(target);
                if (targetPowerLvl <= attacheePowerLvl - 3)
                {
                    return true;
                }
            }

            return false;
        }

        // checks if target is warded from melee damage
        private static bool IsWarded(GameObjectBody obj)
        {
            if (obj.HasCondition(SpellEffects.SpellOtilukesResilientSphere)
                || obj.HasCondition(SpellEffects.SpellMeldIntoStone))
            {
                return true;
            }

            return false;
        }

        private static bool IsSleeping(GameObjectBody obj)
        {
            return obj.HasCondition(SpellEffects.SpellSleep);
        }

        private static bool IsSpiritualWeapon(GameObjectBody obj)
        {
            return obj.D20Query(D20DispatcherKey.QUE_Critter_Has_Spell_Active, WellKnownSpells.SpiritualWeapon, 1);
        }

        private static int GetPowerLevel(GameObjectBody critter)
        {
            var lvl = GameSystems.Critter.GetHitDiceNum(critter);
            var crAdj = critter.GetInt32(obj_f.npc_challenge_rating);
            var objHpMax = critter.GetStat(Stat.hp_max);
            if (objHpMax <= 6)
            {
                crAdj -= 3;
            }
            else if (objHpMax <= 10)
            {
                crAdj -= 2;
            }
            else if (objHpMax <= 15)
            {
                crAdj -= 1;
            }

            return (crAdj + lvl);
        }
    }
}