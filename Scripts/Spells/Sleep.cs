
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
    [SpellScript(438)]
    public class Sleep : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
            // HTN - sort the list by hitdice
            spell.SortTargets(TargetListOrder.HitDiceThenDist, TargetListOrderDirection.Ascending);
            Logger.Info("target_list sorted by hitdice and dist from target_Loc (least to greatest): {0}", spell.Targets);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            spell.duration = 10 * spell.casterLevel;
            var hit_dice_max = 4;
            // hit_dice_roll = dice_new( '2d4' )
            // hit_dice_max = hit_dice_roll.roll()
            // print "sleep, can affect a total of (", hit_dice_max, ") HD"
            SpawnParticles("sp-Sleep", spell.aoeCenter);
            // get all targets in a 15ft radius
            foreach (var target_item in spell.Targets)
            {
                // check critter_hit_dice
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
                if ((obj_hit_dice < 5) && (hit_dice_max >= obj_hit_dice))
                {
                    // subtract the obj.hit_dice from the max
                    hit_dice_max = hit_dice_max - obj_hit_dice;
                    // allow Will saving throw to negate
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                        remove_list.Add(target_item.Object);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        // if game.leader.map == 5005 and target_item.obj.name in range(14074, 14078):
                        // a123 = target_item.obj.item_worn_at(3).obj_get_int(obj_f_weapon_type)
                        // if a123 in [14, 17, 46,  48, 68]: # (Is archer)
                        // snorer = target_item.obj
                        // pad3 = snorer.obj_get_int(obj_f_npc_pad_i_3)
                        // pad3 |= 2**9
                        // snorer.obj_set_int(obj_f_npc_pad_i_3, pad3)
                        // snorer.obj_set_int(obj_f_pad_i_0, snorer.obj_get_int(obj_f_critter_strategy) ) # Record original strategy
                        // snorer.obj_set_int(obj_f_critter_strategy, 88) # "Archer stay put" strat
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.AddCondition("sp-Sleep", spell.spellId, spell.duration, 0);
                    }

                }
                else
                {
                    // Added by Sitra Achara - Moathouse "Wakey Wakey" scripting	#
                    // obj_f_npc_pad_i_3 - bit "7" (2**7)				#
                    // if game.leader.map == 5005 and target_item.obj.name in range(14074, 14078):
                    // snorer = target_item.obj
                    // pad3 = snorer.obj_get_int(obj_f_npc_pad_i_3)
                    // pad3 |= 2**7
                    // snorer.obj_set_int(obj_f_npc_pad_i_3, pad3)
                    // no longer necessary with TemplePLus
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell(); // changed back force_spell_end to 0 despite there being no per-round effects. it caused problems - sp-Sleep would get pruned when saving...
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sleep OnEndSpellCast");
        }

    }
}
