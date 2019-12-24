
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
    [SpellScript(333)]
    public class ObscuringMist : BaseSpellScript
    {
        private static readonly string OBSCURING_MIST_KEY = "Sp333_Obscuring_Mist_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Obscuring Mist OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Obscuring Mist OnSpellEffect");
            spell.duration = 100 * spell.casterLevel;
            locXY locc_;
            if (spell.caster.GetNameId() == 8002 && SelectedPartyLeader.GetMap() == 5005) // Lareth in Moathouse
            {
                locc_ = new locXY(483, 534);
            }
            else
            {
                locc_ = spell.aoeCenter.location;
            }

            // spawn one spell_object object
            var spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, locc_);
            // add to d20initiative
            var caster_init_value = spell.caster.GetInitiative();
            spell_obj.InitD20Status();
            spell_obj.SetInitiative(caster_init_value);
            // put sp-Obscuring Mist condition on obj
            var spell_obj_partsys_id = AttachParticles("sp-Obscuring Mist", spell_obj);
            spell_obj.AddCondition("sp-Obscuring Mist", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
            // Added by Sitra Achara	#
            spell_obj.SetInt(obj_f.secretdoor_dc, 333 + (1 << 15));
            // Mark it as an "obscuring mist" object.
            // 1<<15 - marks it as "active"
            // bits 16 and onward - random ID number

            Co8PersistentData.AddToSpellActiveList(OBSCURING_MIST_KEY, spell.spellId, spell_obj);
        }
        // End of Section		#
        // spell_obj.condition_add_arg_x( 3, spell_obj_partsys_id )
        // objectevent_id = spell_obj.condition_get_arg_x( 2 )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Obscuring Mist OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Obscuring Mist OnEndSpellCast");

            Co8PersistentData.CleanupActiveSpellTargets(OBSCURING_MIST_KEY, spell.spellId, target => {
                var aaa = target.GetInt32(obj_f.secretdoor_dc);
                aaa &= ~(1 << 15);
                target.SetInt32(obj_f.secretdoor_dc, aaa);
            });
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Obscuring Mist OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Obscuring Mist OnSpellStruck");
        }

    }
}
