
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(153)]
    public class Entangle : BaseSpellScript
    {
        private static readonly string ENTANGLE_KEY = "Sp153_Entangle_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Entangle OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Entangle OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            var outdoor_map_list = new[] { 5001, 5002, 5009, 5042, 5043, 5051, 5062, 5068, 5069, 5070, 5071, 5072, 5073, 5074, 5075, 5076, 5077, 5078, 5091, 5093, 5094, 5095, 5096, 5097, 5099, 5100, 5108, 5110, 5111, 5112, 5113, 5119, 5120, 5121, 5132, 5142, 5189 };
            if (outdoor_map_list.Contains(SelectedPartyLeader.GetMap()) || !Co8Settings.EntangleOutdoorsOnly)
            {
                // spawn one Entangle scenery object
                var entangle_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
                // add to d20initiative
                var caster_init_value = spell.caster.GetInitiative();
                entangle_obj.InitD20Status();
                entangle_obj.SetInitiative(caster_init_value);
                // put sp-Entangle condition on obj
                var entangle_partsys_id = AttachParticles("sp-Entangle-Area", entangle_obj);
                entangle_obj.AddCondition("sp-Entangle", spell.spellId, spell.duration, 0, entangle_partsys_id);
                // entangle_obj.condition_add_arg_x( 3, entangle_partsys_id )
                // objectevent_id = entangle_obj.condition_get_arg_x( 2 )
                // Added by Sitra Achara	#
                entangle_obj.SetInt(obj_f.secretdoor_dc, 153 + (1 << 15));
                // Mark it as an "obscuring mist" object.
                // 1<<15 - marks it as "active"
                // bits 16 and onward - random ID number
                Co8PersistentData.AddToSpellActiveList(ENTANGLE_KEY, spell.spellId, entangle_obj);
            }
            else
            {
                // End of Section	#
                // No plants to entangle with
                AttachParticles("Fizzle", spell.caster);
                spell.caster.FloatMesFileLine("mes/spell.mes", 30000);
                spell.caster.FloatMesFileLine("mes/spell.mes", 16014);
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Entangle OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Entangle OnEndSpellCast");
            Co8PersistentData.CleanupActiveSpellTargets(ENTANGLE_KEY, spell.spellId, target =>
            {
                var aaa = target.GetInt32(obj_f.secretdoor_dc);
                aaa &= ~(1 << 15);
                target.SetInt32(obj_f.secretdoor_dc, aaa);
            });
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Entangle OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Entangle OnSpellStruck");
        }

    }
}
