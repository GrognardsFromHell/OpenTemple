
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
    [SpellScript(531)]
    public class Web : BaseSpellScript
    {
        private static readonly string WEB_KEY = "Sp531_Web_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Web OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Web OnSpellEffect");
            spell.duration = 100 * spell.casterLevel;
            // spawn one Web scenery object
            var web_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
            // add to d20initiative
            var caster_init_value = spell.caster.GetInitiative();
            web_obj.InitD20Status();
            web_obj.SetInitiative(caster_init_value);
            // put sp-Web condition on obj
            var Web_partsys_id = AttachParticles("sp-Web", web_obj);
            web_obj.AddCondition("sp-Web", spell.spellId, spell.duration, 0, Web_partsys_id);
            // web_obj.condition_add_arg_x( 3, Web_partsys_id )
            // objectevent_id = web_obj.condition_get_arg_x( 2 )
            // Added by Sitra Achara	#
            web_obj.SetInt(obj_f.secretdoor_dc, 531 + (1 << 15));
            // Mark it as an "obscuring mist" object.
            // 1<<15 - marks it as "active"
            // bits 16 and onward - random ID number
            Co8PersistentData.AddToSpellActiveList(WEB_KEY, spell.spellId, web_obj);
        }
        // End of Section		#

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Web OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Web OnEndSpellCast");
            Co8PersistentData.CleanupActiveSpellTargets(WEB_KEY, spell.spellId, target =>
            {
                var aaa = target.GetInt32(obj_f.secretdoor_dc);
                aaa &= ~(1 << 15);
                target.SetInt32(obj_f.secretdoor_dc, aaa);
            });
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Web OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Web OnSpellStruck");
        }

    }
}
