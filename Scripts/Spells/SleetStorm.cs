
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

namespace Scripts.Spells
{
    [SpellScript(439)]
    public class SleetStorm : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sleet Storm OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        // restored original effect since the bug with infinite AoO was fixed

        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Sleet Storm OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            // spawn one Sleet Storm scenery object
            var sleet_storm_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
            // add to d20initiative
            var caster_init_value = spell.caster.GetInitiative();
            sleet_storm_obj.InitD20Status();
            sleet_storm_obj.SetInitiative(caster_init_value);
            if (spell.duration < 5) // added so NPC's can cast Sleet Storm
            {
                spell.duration = 5; // added so NPC's can cast Sleet Storm
            }

            // put sp-Sleet Storm condition on obj
            var sleet_storm_partsys_id = AttachParticles("sp-Sleet Storm", sleet_storm_obj);
            sleet_storm_obj.AddCondition("sp-Sleet Storm", spell.spellId, spell.duration, 0, sleet_storm_partsys_id);
        }
        // sleet_storm_obj.condition_add_arg_x( 3, sleet_storm_partsys_id )
        // objectevent_id = sleet_storm_obj.condition_get_arg_x( 2 )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Sleet Storm OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sleet Storm OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Sleet Storm OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Sleet Storm OnSpellStruck");
        }

    }
}
