
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(142)]
    public class Doom : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Doom OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Doom OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            var target = spell.Targets[0];
            var npc = spell.caster; // added so NPC's will choose valid targets
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                if ((target.Object.type == ObjectType.pc || target.Object.type == ObjectType.npc) && !Utilities.critter_is_unconscious(target.Object) && !target.Object.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    npc = spell.caster;
                }
                else
                {
                    SetGlobalFlag(811, false);
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 5 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 10 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 15 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 20 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 25 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 30 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 100 && !Utilities.critter_is_unconscious(obj) && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone) && (obj.type == ObjectType.pc || obj.type == ObjectType.npc))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                }

            }

            if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
            {
                // allow Will saving throw to negate
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target.Object);
                    spell.RemoveTarget(target.Object);
                }
                else
                {
                    // saving throw unsuccessful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // HTN - apply condition DOOM
                    target.Object.AddCondition("sp-Doom", spell.spellId, spell.duration, 0);
                    target.ParticleSystem = AttachParticles("sp-Doom", target.Object);
                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Doom OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Doom OnEndSpellCast");
        }

    }
}
