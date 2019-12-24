
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
    [SpellScript(462)]
    public class Stoneskin : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Stoneskin OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Stoneskin OnSpellEffect");
            spell.duration = 100 * spell.casterLevel;
            var damage_max = 10 * Math.Min(15, spell.casterLevel);
            var npc = spell.caster; // added so NPC's can pre-buff
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && !GameSystems.Combat.IsCombatActive())
            {
                spell.duration = 2000 * spell.casterLevel;
            }

            npc = spell.caster; // added so NPC's can use wand/potion/scroll
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && spell.duration <= 0)
            {
                spell.casterLevel = 10;
                damage_max = 100;
                spell.duration = 1000;
            }

            var target = spell.Targets[0];
            if (npc.GetNameId() == 14609)
            {
                spell.casterLevel = 8;
            }

            if (npc.GetNameId() == 14601)
            {
                spell.casterLevel = 4;
            }

            if (npc.GetNameId() == 14241)
            {
                damage_max = 60;
            }

            var partsys_id = AttachParticles("sp-Stoneskin", target.Object);
            if (target.Object.IsFriendly(spell.caster))
            {
                target.Object.AddCondition("sp-Stoneskin", spell.spellId, spell.duration, partsys_id, damage_max);
            }
            // target.obj.condition_add( 'sp-Stoneskin' )
            // target.obj.condition_add_arg_spell_id( spell.id )
            // target.obj.condition_add_arg_duration( spell.duration )
            // target.obj.condition_add_arg_partsys_id( partsys_id )
            // target.obj.condition_add_arg_x( 3, damage_max )
            else if (!target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccessful
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target.Object.AddCondition("sp-Stoneskin", spell.spellId, spell.duration, partsys_id, damage_max);
            }
            else
            {
                // target.obj.condition_add( 'sp-Stoneskin' )
                // target.obj.condition_add_arg_spell_id( spell.id )
                // target.obj.condition_add_arg_duration( spell.duration )
                // target.obj.condition_add_arg_partsys_id( partsys_id )
                // target.obj.condition_add_arg_x( 3, damage_max )
                // saving throw successful
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Stoneskin OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Stoneskin OnEndSpellCast");
        }

    }
}
