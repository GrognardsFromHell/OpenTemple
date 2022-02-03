
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
    [SpellScript(79)]
    public class ControlPlants : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnSpellEffect");
            // Dar's level check no longer needed thanks to Spellslinger's dll fix
            // if spell.caster_class == 14:
            // if spell.spell_level < 3:#added to check for proper ranger slot level (darmagon)
            // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
            // spell.spell_end(spell.id)
            // return
            var remove_list = new List<GameObject>();
            spell.duration = 60 * spell.casterLevel;
            // spawn one control_plants scenery object
            var control_plants_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
            // add to d20initiative
            var caster_init_value = spell.caster.GetInitiative();
            control_plants_obj.InitD20Status();
            control_plants_obj.SetInitiative(caster_init_value);
            // put sp-Control Plants condition on obj
            var control_plants_partsys_id = AttachParticles("sp-Control Plants", control_plants_obj);
            control_plants_obj.AddCondition("sp-Control Plants", spell.spellId, spell.duration, 0, control_plants_partsys_id);
            // control_plants_obj.condition_add_arg_x( 3, control_plants_partsys_id )
            // objectevent_id = control_plants_obj.condition_get_arg_x( 2 )
            // add wilderness_lord bonus to spell_caster
            spell.caster.AddCondition("sp-Control Plants Tracking", spell.spellId, spell.duration, 0);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnSpellStruck");
        }

    }
}
