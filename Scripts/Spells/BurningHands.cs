
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
    [SpellScript(45)]
    public class BurningHands : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Burning Hands OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Burning Hands OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var dam = Dice.D4;
            dam = dam.WithCount(Math.Min(5, spell.casterLevel));
            if (spell.caster.GetNameId() == 14540) // Hell Hound's Breath Weapon
            {
                dam = Dice.Parse("2d6");
                spell.dc = 13;
            }

            var (xx, yy) = spell.caster.GetLocation();
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                AttachParticles("swirled gas", spell.caster);
                spell.caster.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                Sound(7581, 1);
                Sound(7581, 1);
            }
            else
            {
                // else: # caster himself is outside Chamber, now check the targets:
                var soundfizzle = 0;
                AttachParticles("sp-Burning Hands", spell.caster);
                var npc = spell.caster;
                // Caster is NOT in game party
                if (npc.type != ObjectType.pc && npc.GetLeader() == null)
                {
                    var range = 15;
                    var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 150);
                    foreach (var obj in target_list)
                    {
                        if (obj == spell.caster)
                        {
                            continue;
                        }

                        (xx, yy) = obj.GetLocation();
                        if (obj.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                        {
                            obj.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                            SpawnParticles("swirled gas", obj.GetLocation());
                            soundfizzle = 1;
                        }
                        else
                        {
                            if (obj.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                obj.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                obj.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }

                    }

                }

                // Caster is in game party
                if (npc.type == ObjectType.pc || npc.GetLeader() != null)
                {
                    // get all targets in a 10ft cone (180')
                    soundfizzle = 0;
                    foreach (var target_item in spell.Targets)
                    {
                        remove_list.Add(target_item.Object);
                        (xx, yy) = target_item.Object.GetLocation();
                        if (target_item.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                        {
                            // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                            target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                            SpawnParticles("swirled gas", target_item.Object.GetLocation());
                            soundfizzle = 1;
                            spell.RemoveTarget(target_item.Object);
                        }
                        else
                        {
                            if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }

                    }

                }

                if (soundfizzle == 1)
                {
                    Sound(7581, 1);
                    Sound(7581, 1);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Burning Hands OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Burning Hands OnEndSpellCast");
        }

    }
}
