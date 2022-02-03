
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

[SpellScript(490)]
public class TashasHideousLaughter : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Tasha's Hideous Laughter OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Tasha's Hideous Laughter OnSpellEffect");
        spell.duration = 1 * spell.casterLevel;
        var target = spell.Targets[0];
        var npc = spell.caster; // added so NPC's will choose valid targets
        if (npc.GetNameId() == 14333)
        {
            spell.duration = 5;
            spell.casterLevel = 5;
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                target.Object = obj;
            }

        }

        // if npc.name == 14424 and target.obj.stat_level_get( stat_intelligence ) < 3:
        if (npc.type != ObjectType.pc && npc.GetLeader() == null && (target.Object.GetStat(Stat.intelligence) < 3 || Utilities.critter_is_unconscious(target.Object) || target.Object.D20Query(D20DispatcherKey.QUE_Prone)))
        {
            SetGlobalFlag(811, false);
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 5 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 10 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 15 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 20 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 25 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 30 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 35 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 40 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 45 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 50 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(npc) <= 100 && !Utilities.critter_is_unconscious(obj) && obj.GetStat(Stat.intelligence) >= 3 && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    target.Object = obj;
                    SetGlobalFlag(811, true);
                }

            }

        }

        if ((target.Object.GetStat(Stat.intelligence) < 3))
        {
            // print target.obj, " unaffected! (int < 3)"
            target.Object.FloatMesFileLine("mes/spell.mes", 30000);
            target.Object.FloatMesFileLine("mes/spell.mes", 31014);
            // not affected
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }
        else
        {
            // if monster type of caster and target differ, +4 to save (or -4 to DC)
            if (!(target.Object.GetMonsterCategory() == spell.caster.GetMonsterCategory()))
            {
                Logger.Info("category types differ for {0} and {1}!", spell.caster, target.Object);
                spell.dc = spell.dc - 4;
            }

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
                target.Object.AddCondition("sp-Tashas Hideous Laughter", spell.spellId, spell.duration, 0);
                target.ParticleSystem = AttachParticles("sp-Tashas Hideous Laughter", target.Object);
            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Tasha's Hideous Laughter OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Tasha's Hideous Laughter OnEndSpellCast");
    }

}