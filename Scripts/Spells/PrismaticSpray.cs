
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
    [SpellScript(362)]
    public class PrismaticSpray : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Prismatic Spray OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Prismatic Spray OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            AttachParticles("sp-Prismatic Spray", spell.caster);
            var dam = Dice.Parse("1d1");
            var npc = spell.caster;
            var effect1 = 0;
            var effect_list = new List<GameObjectBody>();
            spell.duration = 1000;
            // Caster is NOT in game party
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                var range = 60;
                var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 60);
                target_list.remove/*ObjectList*/(spell.caster);
                // get all targets in a 25ft + 2ft/level cone (60')
                var soundfizzle = 0;
                foreach (var t in target_list)
                {
                    effect_list = new List<GameObjectBody>();
                    effect1 = RandomRange(1, 8);
                    if (effect1 == 8)
                    {
                        effect_list.Add(RandomRange(1, 8));
                        effect_list.Add(RandomRange(1, 8));
                    }
                    else
                    {
                        effect_list.Add(effect1);
                    }

                    if (effect_list.Count == 1)
                    {
                        if (effect_list[0] == 5 || effect_list[0] == 6)
                        {

                        }
                        else
                        {
                            remove_list.Add(t);
                        }

                    }

                    if (effect_list.Count == 2)
                    {
                        if (effect_list[0] == 5 || effect_list[0] == 6 || effect_list[1] == 5 || effect_list[1] == 6)
                        {

                        }
                        else
                        {
                            remove_list.Add(t);
                        }

                    }

                    foreach (var effect in effect_list)
                    {
                        if (effect == 1)
                        {
                            // 20 fire damage
                            dam = dam.WithCount(20);
                            var (xx, yy) = t.GetLocation();
                            if (t.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                            {
                                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                                t.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                                AttachParticles("swirled gas", t);
                                soundfizzle = 1;
                            }
                            else
                            {
                                if (t.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                                {
                                    // saving throw successful
                                    t.FloatMesFileLine("mes/spell.mes", 30001);
                                }
                                else
                                {
                                    // saving throw unsuccessful
                                    t.FloatMesFileLine("mes/spell.mes", 30002);
                                }

                            }

                        }
                        else if (effect == 2)
                        {
                            // 40 acid damage
                            dam = dam.WithCount(40);
                            if (t.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Acid, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                t.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                t.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }
                        else if (effect == 3)
                        {
                            // 80 Electricity damage
                            dam = dam.WithCount(80);
                            if (t.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                t.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                t.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }
                        else if (effect == 4)
                        {
                            // poisoned no save = die save = 1-10 con damage
                            if (!t.IsMonsterCategory(MonsterCategory.ooze) && !t.IsMonsterCategory(MonsterCategory.plant) && !t.IsMonsterCategory(MonsterCategory.undead))
                            {
                                // if not t.saving_throw_spell( spell.dc, D20_Save_Fortitude, D20STD_F_NONE, spell.caster, spell.id ):
                                // saving throw unsuccessful
                                t.FloatMesFileLine("mes/spell.mes", 30002);
                                t.KillWithDeathEffect();
                                // else:
                                // saving throw successful
                                var poison_index = 23;
                                var time_to_secondary = 10;
                                var poison_dc = 200;
                                t.FloatMesFileLine("mes/spell.mes", 30001);
                                t.AddCondition("Poisoned", poison_index, time_to_secondary, poison_dc);
                                // t.condition.add_with_args( 'sp-Neutralize Poison', spell.id, spell.duration, 0 )
                                StartTimer(12000, () => end_poison(spell, t)); // don't want secondary damage here
                            }

                        }
                        else if (effect == 5)
                        {
                            // turned to stone
                            if (t.SavingThrow(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, D20ActionType.CAST_SPELL))
                            {
                                // saving throw successful
                                t.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                t.FloatMesFileLine("mes/spell.mes", 30002);
                                // HTN - apply condition HALT (Petrified)
                                t.AddCondition("sp-Command", spell.spellId, spell.duration, 4);
                                AttachParticles("sp-Bestow Curse", t);
                            }

                        }
                        else if (effect == 6)
                        {
                            // insane
                            var dc = spell.dc;
                            if (!t.SavingThrowSpell(dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                t.FloatMesFileLine("mes/spell.mes", 30002);
                                t.AddCondition("sp-Confusion", spell.spellId, spell.duration, 1);
                            }
                            else
                            {
                                t.FloatMesFileLine("mes/spell.mes", 30001);
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

            // Caster is in game party
            if (npc.type == ObjectType.pc || npc.GetLeader() != null)
            {
                var soundfizzle = 0;
                foreach (var t in spell.Targets)
                {
                    effect_list = new List<GameObjectBody>();
                    effect1 = RandomRange(1, 8);
                    if (effect1 == 8)
                    {
                        effect_list.Add(RandomRange(1, 8));
                        effect_list.Add(RandomRange(1, 8));
                    }
                    else
                    {
                        effect_list.Add(effect1);
                    }

                    if (effect_list.Count == 1)
                    {
                        if (effect_list[0] == 5 || effect_list[0] == 6)
                        {

                        }
                        else
                        {
                            remove_list.Add(t.Object);
                        }

                    }

                    if (effect_list.Count == 2)
                    {
                        if (effect_list[0] == 5 || effect_list[0] == 6 || effect_list[1] == 5 || effect_list[1] == 6)
                        {

                        }
                        else
                        {
                            remove_list.Add(t.Object);
                        }

                    }

                    foreach (var effect in effect_list)
                    {
                        if (effect == 1)
                        {
                            // 20 fire damage
                            dam = dam.WithCount(20);
                            var (xx, yy) = t.Object.GetLocation();
                            if (t.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                            {
                                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                                t.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                                AttachParticles("swirled gas", t.Object);
                                soundfizzle = 1;
                            }
                            else
                            {
                                if (t.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                                {
                                    // saving throw successful
                                    t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                                }
                                else
                                {
                                    // saving throw unsuccessful
                                    t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                }

                            }

                        }
                        else if (effect == 2)
                        {
                            // 40 acid damage
                            dam = dam.WithCount(40);
                            if (t.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Acid, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }
                        else if (effect == 3)
                        {
                            // 80 Electricity damage
                            dam = dam.WithCount(80);
                            if (t.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }
                        else if (effect == 4)
                        {
                            // poisoned no save = die save = 1-10 con damage
                            if (!t.Object.IsMonsterCategory(MonsterCategory.ooze) && !t.Object.IsMonsterCategory(MonsterCategory.plant) && !t.Object.IsMonsterCategory(MonsterCategory.undead) && !t.Object.IsMonsterCategory(MonsterCategory.construct))
                            {
                                if (!t.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                                {
                                    // saving throw unsuccessful
                                    t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                    // So you'll get awarded XP for the kill
                                    if (!((SelectedPartyLeader.GetPartyMembers()).Contains(t.Object)))
                                    {
                                        t.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                                    }

                                    t.Object.KillWithDeathEffect();
                                }
                                else
                                {
                                    // saving throw successful
                                    var poison_index = 23;
                                    var time_to_secondary = 10;
                                    var poison_dc = 200;
                                    t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                                    t.Object.AddCondition("Poisoned", poison_index, time_to_secondary, poison_dc);
                                    StartTimer(6000, () => end_poison(spell, t.Object)); // don't want secondary damage here
                                }

                            }

                        }
                        else if (effect == 5)
                        {
                            // turned to stone
                            if (t.Object.SavingThrow(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, D20ActionType.CAST_SPELL))
                            {
                                // saving throw successful
                                t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                // HTN - apply condition HALT (Petrified)
                                t.Object.AddCondition("sp-Command", spell.spellId, spell.duration, 4);
                                AttachParticles("sp-Bestow Curse", t.Object);
                            }

                        }
                        else if (effect == 6)
                        {
                            // insane
                            var dc = spell.dc;
                            if (!t.Object.SavingThrowSpell(dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                t.Object.AddCondition("sp-Confusion", spell.spellId, spell.duration, 1);
                            }
                            else
                            {
                                t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            }

                        }
                        else if (effect == 7)
                        {
                            // sent to another plane- kill and destroy for now
                            var dc = spell.dc;
                            if (!t.Object.SavingThrowSpell(dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                t.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                // So you'll get awarded XP for the kill
                                if (!((SelectedPartyLeader.GetPartyMembers()).Contains(t.Object)))
                                {
                                    t.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                                }

                                t.Object.KillWithDeathEffect();
                                t.Object.Destroy();
                            }
                            else
                            {
                                t.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            }

                        }

                    }

                FIXME: DEL effect_list;

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
            Logger.Info("Prismatic Spray OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Prismatic Spray OnEndSpellCast");
        }
        public static void end_poison(SpellPacketBody spell, FIXME id)
        {
            id.condition_add_with_args/*Unknown*/("sp-Neutralize Poison", spell.spellId, spell.duration, 0);
        }

    }
}
