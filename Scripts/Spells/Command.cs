
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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

namespace Scripts.Spells;

[SpellScript(67)]
public class Command : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Command OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Command OnSpellEffect");
        spell.duration = 1;
        var target_item = spell.Targets[0];
        var npc = spell.caster; // added so NPC's will choose valid targets
        // if npc.name == 14424 or npc.name = 8091:			##  added so NPC's will choose valid targets
        if (npc.type != ObjectType.pc && npc.GetLeader() == null)
        {
            if (!target_item.Object.IsMonsterCategory(MonsterCategory.animal) && (GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) < SizeCategory.Large || target_item.Object.IsMonsterCategory(MonsterCategory.humanoid)) && !Utilities.critter_is_unconscious(target_item.Object) && !target_item.Object.D20Query(D20DispatcherKey.QUE_Prone))
            {
                npc = spell.caster;
            }
            else
            {
                SetGlobalFlag(811, false);
                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 5 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 10 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 15 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 20 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 25 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 30 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if (obj.DistanceTo(npc) <= 100 && !Utilities.critter_is_unconscious(obj) && !obj.IsMonsterCategory(MonsterCategory.animal) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        target_item.Object = obj;
                        SetGlobalFlag(811, true);
                    }

                }

            }

        }

        // Solves Radial menu problem for Wands/NPCs
        var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
        if (spell_arg != 1 && spell_arg != 2 && spell_arg != 3 && spell_arg != 4)
        {
            spell_arg = 2;
        }

        if (npc.type != ObjectType.pc && npc.GetLeader() == null)
        {
            spell_arg = 2;
        }

        if (!target_item.Object.IsFriendly(spell.caster))
        {
            if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
            {
                if (!target_item.Object.IsMonsterCategory(MonsterCategory.animal))
                {
                    if ((GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) < SizeCategory.Large || target_item.Object.IsMonsterCategory(MonsterCategory.humanoid)))
                    {
                        if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            // saving throw unsuccessful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
                            {
                                target_item.Object.AddCondition("sp-Command", spell.spellId, spell.duration, 2);
                            }
                            else
                            {
                                // else:			##  added so NPC's can cast Command
                                target_item.Object.AddCondition("sp-Command", spell.spellId, spell.duration, spell_arg);
                            }

                            target_item.ParticleSystem = AttachParticles("sp-Command", target_item.Object);
                        }
                        else
                        {
                            // add target to initiative, just in case
                            // target_item.obj.add_to_initiative()
                            // game.update_combat_ui()
                            // saving throw successful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            AttachParticles("Fizzle", target_item.Object);
                            spell.RemoveTarget(target_item.Object);
                        }

                    }
                    else
                    {
                        // not medium sized or smaller
                        // Note: I've added an exception for humanoids (i.e. magically enlarged party members) -SA
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 31005);
                        AttachParticles("Fizzle", target_item.Object);
                        spell.RemoveTarget(target_item.Object);
                    }

                }
                else
                {
                    // a monster
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31004);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }
            else
            {
                // not a person
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31001);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

        }
        else
        {
            // can't target friendlies
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
            AttachParticles("Fizzle", target_item.Object);
            spell.RemoveTarget(target_item.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Command OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Command OnEndSpellCast");
    }

}