
using System;
using System.Collections.Generic;
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
    [SpellScript(152)]
    public class Enlarge : BaseSpellScript
    {
        private static readonly string ENLARGE_KEY = "Sp152_Enlarge_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Enlarge OnBeginSpellCast");
            // print "\nspell.target_list=" , str(spell.target_list) , "\n"
            // print "\nspell.caster=" + str( spell.caster) + " caster.level= ", spell.caster_level , "\n"
            // print "\nspell.id=", spell.id , "\n"
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Enlarge OnSpellEffect");
            // print "spell.id=", spell.id
            // print "spell.target_list=", spell.target_list
            spell.duration = 10 * spell.casterLevel;
            var target_item = spell.Targets[0];
            // HTN - 3.5, enlarge PERSON only
            if ((target_item.Object.IsMonsterCategory(MonsterCategory.humanoid)))
            {
                if (target_item.Object.IsFriendly(spell.caster))
                {
                    var return_val = target_item.Object.AddCondition("sp-Enlarge", spell.spellId, spell.duration, 0);
                    // print "condition_add_with_args return_val: " + str(return_val) + "\n"
                    if (return_val)
                    {
                        // size mod
                        // print "Size:" + str(target_item.obj.obj_get_int(obj_f_size))
                        // print "Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
                        SizeUtils.IncSizeCategory(target_item.Object);
                        // print "performed size Increase\n"
                        // save target_list

                        Co8PersistentData.AddToSpellActiveList(ENLARGE_KEY, spell.spellId, target_item.Object);

                        // print "new Size:" + str(target_item.obj.obj_get_int(obj_f_size))
                        // print "new Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
                        target_item.ParticleSystem = AttachParticles("sp-Enlarge", target_item.Object);
                    }

                }
                else
                {
                    if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw unsuccesful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        var return_val = target_item.Object.AddCondition("sp-Enlarge", spell.spellId, spell.duration, 0);
                        // enemies seem to work fine?
                        // print "Size:" + str(target_item.obj.obj_get_int(obj_f_size))
                        // print "Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
                        // size.incSizeCategory(target_item.obj)
                        // print "new Size:" + str(target_item.obj.obj_get_int(obj_f_size))
                        // print "new Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
                        if (return_val)
                        {
                            target_item.ParticleSystem = AttachParticles("sp-Enlarge", target_item.Object);
                        }
                        else
                        {
                            // saving throw successful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            AttachParticles("Fizzle", target_item.Object);
                        }

                    }

                }

            }

            // spell.target_list.remove_target( target_item.obj )
            Logger.Info("spell.target_list={0}", spell.Targets);
        }
        // spell.spell_end( spell.id )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Enlarge OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Enlarge OnEndSpellCast");
            // print "spell.target_list=", spell.target_list
            // print "spell.id=", spell.id
            // size mod
            Co8PersistentData.CleanupActiveSpellTargets(ENLARGE_KEY, spell.spellId, target => {
                // print "Size:" + str(targetObj.obj_get_int(obj_f_size))
                // print "Reach:" + str(targetObj.obj_get_int(obj_f_critter_reach))
                Co8.weap_too_big(target);
                SizeUtils.ResetSizeCategory(target);
            });

        }

    }
}
