
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
    [SpellScript(200)]
    public class Grease : BaseSpellScript
    {
        private static readonly string GREASE_KEY = "Sp200_Grease_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Grease OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Grease OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            // spawn one spell_object object
            var spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
            // add to d20initiative
            var caster_init_value = spell.caster.GetInitiative();
            spell_obj.InitD20Status();
            spell_obj.SetInitiative(caster_init_value);
            // put sp-Grease condition on obj
            var spell_obj_partsys_id = AttachParticles("sp-Grease", spell_obj);
            spell_obj.AddCondition("sp-Grease", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
            // spell_obj.condition_add_arg_x( 3, spell_obj_partsys_id )
            // objectevent_id = spell_obj.condition_get_arg_x( 2 )
            // Added by Sitra Achara	#
            spell_obj.SetInt(obj_f.secretdoor_dc, 200 + (1 << 15));
            // Mark it as a "grease" object.
            // 1<<15 - marks it as "active"
            // bits 16 and onward - random ID number
            var activeList = Co8PersistentData.getData/*Unknown*/(GREASE_KEY);
            if (isNone(activeList))
            {
                activeList = new List<GameObjectBody>();
            }

            activeList.Add(new[] { spell.spellId, derefHandle(spell_obj) });
            Co8PersistentData.setData/*Unknown*/(GREASE_KEY, activeList);
        }
        // End of Section		#

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Grease OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Grease OnEndSpellCast");
            var activeList = Co8PersistentData.getData/*Unknown*/(GREASE_KEY);
            if (isNone(activeList))
            {
                Logger.Info("ERROR! Active Grease spell without activeList!");
                return;
            }

            foreach (var entry in activeList)
            {
                var (spellID, target) = entry;
                var targetObj = refHandle(target);
                if (spellID == spell.spellId)
                {
                    var aaa = targetObj.obj_get_int/*Unknown*/(obj_f.secretdoor_dc);
                    aaa &= ~(1 << 15);
                    targetObj.obj_set_int/*Unknown*/(obj_f.secretdoor_dc, aaa);
                    activeList.remove/*Unknown*/(entry);
                    // no more active spells
                    if (activeList.Count == 0)
                    {
                        Co8PersistentData.removeData/*Unknown*/(GREASE_KEY);
                        break;

                    }

                    Co8PersistentData.setData/*Unknown*/(GREASE_KEY, activeList);
                    break;

                }

            }

        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Grease OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Grease OnSpellStruck");
        }

    }
}
