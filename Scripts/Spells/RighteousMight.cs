
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
    [SpellScript(404)]
    public class RighteousMight : BaseSpellScript
    {
        private static readonly string RM_KEY = "Sp404_RighteousMight_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var target_item = spell.Targets[0];
            // size mod
            // print "Size:" + str(target_item.obj.obj_get_int(obj_f_size))
            // print "Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
            size.incSizeCategory/*Unknown*/(target_item.Object);
            // save target_list
            var activeList = Co8PersistentData.getData/*Unknown*/(RM_KEY);
            if (isNone(activeList))
            {
                activeList = new List<GameObjectBody>();
            }

            activeList.Add(new[] { spell.spellId, derefHandle(target_item.Object) });
            Co8PersistentData.setData/*Unknown*/(RM_KEY, activeList);
            // print "new Size:" + str(target_item.obj.obj_get_int(obj_f_size))
            // print "new Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
            target_item.Object.AddCondition("sp-Righteous Might", spell.spellId, spell.duration, 0);
            target_item.ParticleSystem = AttachParticles("sp-Righteous Might", target_item.Object);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnEndSpellCast");
            // print "spell.target_list=", spell.target_list
            // print "spell.id=", spell.id
            // size mod
            var activeList = Co8PersistentData.getData/*Unknown*/(RM_KEY);
            if (isNone(activeList))
            {
                Logger.Info("ERROR! Active RM spell without activeList!");
                return;
            }

        FIXME: FORELSE
        {
                Logger.Info("ERROR! Active RM spell without entry in activeList!");
            }
        FIXME: FORELSE
        foreach (var entry in activeList)
            {
                var (spellID, target) = entry;
                var targetObj = refHandle(target);
                // print "activeLIst Entry:" + str(spellID)
                if (spellID == spell.spellId)
                {
                    // print "Size:" + str(targetObj.obj_get_int(obj_f_size))
                    // print "Reach:" + str(targetObj.obj_get_int(obj_f_critter_reach))
                    size.resetSizeCategory/*Unknown*/(targetObj);
                    // print "resetting reach on", targetObj
                    // print "new Size:" + str(targetObj.obj_get_int(obj_f_size))
                    // print "new Reach:" + str(targetObj.obj_get_int(obj_f_critter_reach))
                    activeList.remove/*Unknown*/(entry);
                    // no more active spells
                    if (activeList.Count == 0)
                    {
                        Co8PersistentData.removeData/*Unknown*/(RM_KEY);
                        break;

                    }

                    Co8PersistentData.setData/*Unknown*/(RM_KEY, activeList);
                    break;

                }

            }

        }

    }
}
