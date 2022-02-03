
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

namespace Scripts.Spells;

[SpellScript(562)]
public class DeepSlumber : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Deep Slumber OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
        // HTN - sort the list by hitdice
        spell.SortTargets(TargetListOrder.HitDiceThenDist, TargetListOrderDirection.Ascending);
        Logger.Info("target_list sorted by hitdice and dist from target_Loc (least to greatest): {0}", spell.Targets);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Deep Slumber OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 10 * spell.casterLevel;
        var hit_dice_max = 10;
        // print "Deep Slumber, can affect a total of (", hit_dice_max, ") HD"
        SpawnParticles("sp-Deep Slumber", spell.aoeCenter);
        // get all targets in a 15ft radius
        foreach (var target_item in spell.Targets)
        {
            // check critter_hit_dice
            var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
            if ((obj_hit_dice < 11) && (hit_dice_max >= obj_hit_dice))
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
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target_item.Object.AddCondition("sp-Sleep", spell.spellId, spell.duration, 0);
                }

            }
            else
            {
                AttachParticles("Fizzle", target_item.Object);
                remove_list.Add(target_item.Object);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Deep Slumber OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Deep Slumber OnEndSpellCast");
    }

}