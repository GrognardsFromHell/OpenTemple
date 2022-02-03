
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

[SpellScript(297)]
public class MassCharmMonster : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Mass Charm Monster OnBeginSpellCast {0}", spell.spellId);
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
        // Sort the list by hitdice
        spell.SortTargets(TargetListOrder.HitDiceThenDist, TargetListOrderDirection.Ascending);
        Logger.Info("target_list sorted by hitdice and dist from target_Loc (least to greatest): {0}", spell.Targets);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Mass Charm Monster OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 600 * spell.casterLevel;
        var hitDiceAmount = 2 * spell.casterLevel;
        if (Co8Settings.CharmSpellDCModifier)
        {
            if (GameSystems.Combat.IsCombatActive())
            {
                spell.dc = spell.dc - 5; // to reflect a bonus to the saving throw for casting charm in combat
            }

        }

        foreach (var target in spell.Targets)
        {
            // Check critter hit dice
            var targetHitDice = GameSystems.Critter.GetHitDiceNum(target.Object);
            if ((targetHitDice <= hitDiceAmount || target == spell.Targets[0]))
            {
                // Subtract the target's hit dice from the amount allowed.
                hitDiceAmount = hitDiceAmount - targetHitDice;
                if (!target.Object.IsFriendly(spell.caster))
                {
                    if (!target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.CHARM, spell.caster, spell.spellId))
                    {
                        // saving throw unsuccessful
                        target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        spell.caster.AddAIFollower(target.Object);
                        target.Object.AddCondition("sp-Charm Monster", spell.spellId, spell.duration, GameSystems.Critter.GetHitDiceNum(target.Object));
                        target.ParticleSystem = AttachParticles("sp-Charm Monster", target.Object);
                        // add target to initiative
                        target.Object.AddToInitiative();
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                        // Add time event.
                        StartTimer(spell.duration * 6000, () => removeCharmMonster(spell.caster, target.Object));
                    }
                    else
                    {
                        // saving throw successful
                        target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target.Object);
                        remove_list.Add(target.Object);
                    }

                }
                else
                {
                    // can't target friendlies
                    AttachParticles("Fizzle", target.Object);
                    remove_list.Add(target.Object);
                }

            }
            else
            {
                // Run out of allowed HD.
                AttachParticles("Fizzle", target.Object);
                remove_list.Add(target.Object);
            }

        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Mass Charm Monster OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Mass Charm Monster OnEndSpellCast");
    }
    public void removeCharmMonster(GameObject caster, GameObject target)
    {
        Logger.Info("Mass Charm Monster - removing charm. {0}{1}", caster, target);
        caster.RemoveAIFollower(target);
        caster.RemoveFollower(target);
        caster.AIAddToShitlist(target);
    }

}