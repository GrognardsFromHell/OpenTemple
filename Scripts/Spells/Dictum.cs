
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
    [SpellScript(121)]
    public class Dictum : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dictum OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Dictum OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            // The Will save versus banishment is at a -4 penalty
            spell.dc = spell.dc + 4;
            // ff = open('dictum.txt', 'w')
            // ffs = ''
            var target_list = Utilities.pyspell_targetarray_copy_to_obj_list(spell);
            foreach (var target_item_obj in target_list)
            {
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item_obj);
                // Only works on non-lawful creatures
                var alignment = target_item_obj.GetAlignment();
                if (!(alignment.IsLawful()))
                {
                    AttachParticles("sp-Destroy Undead", target_item_obj);
                    // Anything ten or more levels below the caster's level dies
                    // ffs += '\n' + str(spell.caster_level) + '\n' + str(obj_hit_dice) + '\n'
                    if (obj_hit_dice <= (spell.casterLevel - 10))
                    {
                        // So you'll get awarded XP for the kill
                        if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item_obj)))
                        {
                            target_item_obj.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                        }

                        target_item_obj.Kill();
                    }

                    // Anything five or more levels below the caster's level is paralyzed
                    if (obj_hit_dice <= (spell.casterLevel - 5))
                    {
                        spell.duration = RandomRange(1, 10) * 10;
                        target_item_obj.AddCondition("sp-Hold Monster", spell.spellId, spell.duration, 0);
                    }

                    // Anything one or more levels below the caster's level is slowed
                    if (obj_hit_dice <= (spell.casterLevel - 1))
                    {
                        spell.duration = RandomRange(1, 4) + RandomRange(1, 4);
                        target_item_obj.AddCondition("sp-Slow", spell.spellId, spell.duration, 1);
                    }

                    // Anything the caster's level or below is deafened
                    if (obj_hit_dice <= (spell.casterLevel))
                    {
                        spell.duration = RandomRange(1, 4);
                        target_item_obj.AddCondition("sp-Shout", spell.spellId, spell.duration, 0);
                        // Summoned and extraplanar creatures below the caster's level are also banished
                        // if they fail a Will save at -4
                        if (target_item_obj.HasCondition(SpellEffects.SpellSummoned) || (target_item_obj.GetNpcFlags() & NpcFlag.EXTRAPLANAR) != 0)
                        {
                            // ffs += 'Spell condition summoned: ' + str(target_item_obj.d20_query_has_spell_condition( sp_Summoned ) ) + '\nExtraplanar: ' + str(target_item_obj.npc_flags_get( ONF_EXTRAPLANAR )) + '\n'
                            // allow Will saving throw to negate
                            if (target_item_obj.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                            {
                                // saving throw successful
                                target_item_obj.FloatMesFileLine("mes/spell.mes", 30001);
                                AttachParticles("Fizzle", target_item_obj);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                target_item_obj.FloatMesFileLine("mes/spell.mes", 30002);
                                // creature is sent back to its own plane
                                // kill for now
                                // So you'll get awarded XP for the kill
                                if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item_obj)))
                                {
                                    target_item_obj.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                                }

                                target_item_obj.Kill();
                            }

                        }

                    }

                }

                remove_list.Add(target_item_obj);
            }

            // ff.write(ffs)
            // ff.close()
            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Dictum OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dictum OnEndSpellCast");
        }

    }
}
