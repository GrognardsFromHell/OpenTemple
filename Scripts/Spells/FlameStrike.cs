
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

namespace Scripts.Spells
{
    [SpellScript(178)]
    public class FlameStrike : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnSpellEffect");
            var remove_list = new List<GameObject>();
            // damage is split between FIRE and DIVINE damage
            var dam = Dice.D6;
            dam = dam.WithCount(Math.Min(15, spell.casterLevel));
            var dam_over2 = Dice.Constant(0);
            dam_over2 = dam_over2.WithModifier(dam.Roll() / 2);
            var (xx, yy) = spell.caster.GetLocation(); // caster is in chamber
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                AttachParticles("swirled gas", spell.caster);
                spell.caster.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                Sound(7581, 1);
                Sound(7581, 1);
                foreach (var target_item in spell.Targets)
                {
                    remove_list.Add(target_item.Object);
                }

                spell.EndSpell();
                return;
            }

            (xx, yy) = spell.aoeCenter.location; // center of targeting circle in chamber
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                var tro = GameSystems.MapObject.CreateObject(14070, spell.aoeCenter);
                SpawnParticles("swirled gas", spell.aoeCenter);
                spell.caster.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                tro.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                tro.Destroy();
                Sound(7581, 1);
                Sound(7581, 1);
                foreach (var target_item in spell.Targets)
                {
                    remove_list.Add(target_item.Object);
                }

                spell.EndSpell();
                return;
            }

            SpawnParticles("sp-Flame Strike", spell.aoeCenter);
            // get all targets in a 10ft radius
            var soundfizzle = 0;
            foreach (var target_item in spell.Targets)
            {
                (xx, yy) = target_item.Object.GetLocation(); // center of targeting circle in chamber
                if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                {
                    // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                    SpawnParticles("swirled gas", target_item.Object.GetLocation());
                    target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                    soundfizzle = 1;
                    spell.RemoveTarget(target_item.Object);
                    remove_list.Add(target_item.Object);
                    continue;

                }

                // do reflex saving throw on FIRE damage, then do damage/save check for DIVINE damage
                if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam_over2, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    // check for evasion
                    if ((!target_item.Object.HasFeat(FeatId.EVASION)) && (!target_item.Object.HasFeat(FeatId.IMPROVED_EVASION)))
                    {
                        // saving throw successful, apply half damage
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, dam_over2, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    if (!target_item.Object.HasFeat(FeatId.IMPROVED_EVASION))
                    {
                        // saving throw unsuccessful, apply full damage
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, dam_over2, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        // saving throw successful, apply half damage because of IMPROVED EVASION
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, dam_over2, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

                remove_list.Add(target_item.Object);
            }

            if (soundfizzle == 1)
            {
                Sound(7581, 1);
                Sound(7581, 1);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnEndSpellCast");
        }

    }
}
