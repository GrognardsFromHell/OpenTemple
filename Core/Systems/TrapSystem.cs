using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems
{
    [Flags]
    public enum TrapSpecFlag
    {
        TF_IN_STONE,
        TF_PC,
        TF_SPOTTED,
        TF_MAGICAL
    }

    public readonly struct TrapDamage
    {
        public readonly DamageType Type;
        public readonly Dice Dice;
    }

    public class Trap
    {
        // last column in trap.tab seems to be the numeric trap id, stored in "counters" field of san_trap
        public int Id { get; }

        public string Name { get; } // first col

        // Which script event triggers it?
        public ObjScriptEvent Trigger { get; }
        public TrapSpecFlag Flags { get; }
        public string ParticleSystemId { get; }
        public int SearchDC { get; }
        public int DisarmDC { get; }

        // If the obj is not a "real" trap, the trap script will be replaced by this trap after triggering (by name)
        public string AfterTriggerName { get; }

        public int ChallengeRating { get; } // second to last col
        public IImmutableList<TrapDamage> Damage { get; }
    }

    public class TrapSystem : IGameSystem
    {
        [TempleDllLocation(0x10aa32a0)]
        private readonly Dictionary<int, string> _translations;

        [TempleDllLocation(0x10050da0)]
        public TrapSystem()
        {
            _translations = Tig.FS.ReadMesFile("mes/trap.mes");

            Stub.TODO();
        }

        [TempleDllLocation(0x10050940)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100514c0)]
        public void AttemptDisarm(GameObject critter, GameObject trap, out bool success)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10050d20)]
        private Trap GetTrapById(int trapId)
        {
            throw new NotImplementedException();
        }

        private bool TryGetTrapFromObject(GameObject trappedObj, out Trap trap)
        {
            var script = GetTrapScript(trappedObj);
            if (script.scriptId == 0)
            {
                trap = null;
                return false;
            }

            var trapId = (int) (script.counters & 0xFF);
            if (trapId <= 0)
            {
                trap = null;
                return false;
            }

            trap = GetTrapById(trapId);
            return trap != null;
        }

        [TempleDllLocation(0x10050e30)]
        public bool WillTrigger(GameObject trappedObj, ObjScriptEvent? triggerEvent = null)
        {
            if (!TryGetTrapFromObject(trappedObj, out var trap))
            {
                return false;
            }

            return !triggerEvent.HasValue || trap.Trigger == triggerEvent.Value;
        }

        [TempleDllLocation(0x10050ea0)]
        public bool WillTriggerForUser(GameObject triggerer, GameObject trappedObj)
        {
            if (triggerer == null || !triggerer.IsCritter() || !WillTrigger(trappedObj))
            {
                return false;
            }

            var script = GetTrapScript(trappedObj);

            var trapFlags = script.unk1;
            if (triggerer.IsPC() || GameSystems.Critter.GetLeaderRecursive(triggerer) != null)
            {
                if ((trapFlags & 6) != 0)
                    return true;
            }
            else if ((trapFlags & 2) == 0)
            {
                return true;
            }

            return false;
        }

        private static ObjectScript GetTrapScript(GameObject trappedObj)
        {
            return trappedObj.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Trap);
        }

        [TempleDllLocation(0x10051350)]
        public bool TryToDetect(GameObject critter, GameObject trappedObj, BonusList searchBonus)
        {
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_CannotUseIntSkill))
            {
                return false;
            }

            if (!WillTriggerForUser(critter, trappedObj))
            {
                return false;
            }

            int dc = 0;
            if (TryGetTrapFromObject(trappedObj, out var trap))
            {
                dc = trap.SearchDC;
                if (dc > 20 && !GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Can_Find_Traps))
                {
                    return false;
                }
            }

            var bonus = critter.dispatch1ESkillLevel(SkillId.search, ref searchBonus, trappedObj,
                SkillCheckFlags.SearchForTraps);
            var roll = Dice.D20.Roll();

            var rollHistId = GameSystems.RollHistory.AddSkillCheck(
                critter, null, SkillId.search, Dice.D20, roll, dc, searchBonus
            );
            if (roll + bonus >= dc)
            {
                GameSystems.RollHistory.CreateRollHistoryString(rollHistId);
                TrapSpotted(critter, trappedObj, false);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x100509c0)]
        public void TrapSpotted(GameObject triggerer, GameObject trappedObj, bool suppressMessage)
        {
            if (triggerer == null)
            {
                return;
            }

            if (triggerer.IsPC() && !GameSystems.Party.IsInParty(triggerer))
            {
                return;
            }

            if (!triggerer.IsNPC() || GameSystems.Critter.GetLeaderRecursive(triggerer) == null)
            {
                return;
            }

            var script = GetTrapScript(trappedObj);
            script.unk1 |= 4;
            if (trappedObj.type == ObjectType.trap)
            {
                GameSystems.MapObject.ClearFlags(trappedObj, ObjectFlag.DONTDRAW);
            }

            if (!suppressMessage)
            {
                var text = _translations[0];
                GameSystems.TextFloater.FloatLine(triggerer, TextFloaterCategory.Generic, TextFloaterColor.Green, text);
            }

            trappedObj.SetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Trap, in script);
        }
    }
}