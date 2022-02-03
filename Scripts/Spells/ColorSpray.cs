
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

[SpellScript(66)]
public class ColorSpray : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Color Spray OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-illusion-conjure", spell.caster);
        var dice = Dice.D6;
        var num_targets_affected = dice.Roll();
        Logger.Info("num_of_targets_affected={0}", num_targets_affected);
        // HTN - sort the list by distance from caster
        spell.SortTargets(TargetListOrder.DistFromCaster, TargetListOrderDirection.Ascending);
        Logger.Info("target_list sorted by dist from [{0}] (closest to farthest): {1}", spell.caster, spell.Targets);
        // remove targets greater than num_affected
        if (spell.Targets.Length > num_targets_affected)
        {
            var remove_list = new List<GameObject>();
            var index = 0;
            foreach (var target_item in spell.Targets)
            {
                if (index >= num_targets_affected)
                {
                    remove_list.Add(target_item.Object);
                }

                index = index + 1;
            }

            spell.RemoveTargets(remove_list);
        }

        Logger.Info("target_list after pruning: {0}", spell.Targets);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Color Spray OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 0;
        AttachParticles("sp-Color Spray", spell.caster);
        foreach (var target_item in spell.Targets)
        {
            if (!target_item.Object.D20Query(D20DispatcherKey.QUE_Critter_Is_Blinded))
            {
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
                    if (GameSystems.Critter.GetHitDiceNum(target_item.Object) >= 5)
                    {
                        target_item.Object.AddCondition("sp-Color Spray Stun", spell.spellId, spell.duration, 0);
                    }
                    else if (GameSystems.Critter.GetHitDiceNum(target_item.Object) >= 3)
                    {
                        target_item.Object.AddCondition("sp-Color Spray Blind", spell.spellId, spell.duration, 0);
                    }
                    else
                    {
                        target_item.Object.AddCondition("sp-Color Spray Unconscious", spell.spellId, spell.duration, 0);
                    }

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
        Logger.Info("Color Spray OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Color Spray OnEndSpellCast");
    }

}