
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
    [SpellScript(26)]
    public class Banishment : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Banishment OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Banishment OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            spell.duration = 0;
            var hitDiceAmount = 2 * spell.casterLevel;
            var banish_casterLV = spell.casterLevel;
            // check for any item that is distasteful to the subjects (Needs suggestions)
            var bonus1_list = new[] { 8028 }; // Potion of Protection From Outsiders
            foreach (var bonus1 in bonus1_list)
            {
                if (spell.caster.FindItemByName(bonus1) != null)
                {
                    spell.dc = spell.dc + 2; // the saving throw DC increases by 2
                }

            }

            // does NOT work! (Needs a fix.)
            // spell.caster_level = spell.caster_level + 1	## +1 bonus on your caster level check for overcoming Spell Resistance
            // check for rare items that work twice as well as a normal item for the purpose of the bonuses (Needs suggestions)
            var bonus2_list = new[] { 12900 }; // Swamp Lotus
            foreach (var bonus2 in bonus2_list)
            {
                if (spell.caster.FindItemByName(bonus2) != null)
                {
                    spell.dc = spell.dc + 4; // the saving throw DC increases by 4
                }

            }

            // does NOT work! (Needs a fix.)
            // spell.caster_level = spell.caster_level + 2	## +2 bonus on your caster level check for overcoming Spell Resistance
            var spell_dc = spell.dc;
            foreach (var target_item in spell.Targets)
            {
                // check critter hit dice
                var targetHitDice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
                // check if target does not exceed the amount allowed
                if (hitDiceAmount >= targetHitDice)
                {
                    // spell.dc is DC - target's HD + caster's caster_level
                    spell.dc = spell_dc - targetHitDice + banish_casterLV;
                    spell.dc = Math.Max(1, spell.dc);
                    if ((target_item.Object.type == ObjectType.npc))
                    {
                        // check target is EXTRAPLANAR
                        if (target_item.Object.HasCondition(SpellEffects.SpellSummoned) || target_item.Object.IsMonsterCategory(MonsterCategory.outsider) || target_item.Object.IsMonsterSubtype(MonsterSubtype.extraplanar))
                        {
                            // subtract the target's hit dice from the amount allowed
                            hitDiceAmount = hitDiceAmount - targetHitDice;
                            // allow Will saving throw to negate
                            if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                // saving throw successful
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                                AttachParticles("Fizzle", target_item.Object);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                                // creature is sent back to its own plane (no 20% of wrong plane, DUMB)
                                // kill for now
                                // So you'll get awarded XP for the kill
                                if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                                {
                                    target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                                }

                                target_item.Object.Kill();
                                if (target_item.Object.HasCondition(SpellEffects.SpellSummoned))
                                {
                                    SpawnParticles("sp-Dismissal", target_item.Object.GetLocation());
                                    target_item.Object.Destroy();
                                }
                                else
                                {
                                    AttachParticles("sp-Dismissal", target_item.Object);
                                    target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, 3);
                                }

                            }

                        }
                        else
                        {
                            // target is not EXTRAPLANAR
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 31007);
                            AttachParticles("Fizzle", target_item.Object);
                        }

                    }
                    else
                    {
                        // target is not an NPC
                        AttachParticles("Fizzle", target_item.Object);
                    }

                }
                else
                {
                    // ran out of allowed HD
                    AttachParticles("Fizzle", target_item.Object);
                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Banishment OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Banishment OnEndSpellCast");
        }

    }
}
