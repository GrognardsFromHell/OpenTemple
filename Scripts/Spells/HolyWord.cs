
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
    [SpellScript(233)]
    public class HolyWord : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Holy Word OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Holy Word OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            // The Will save versus banishment is at a -4 penalty
            spell.dc = spell.dc + 4;
            var obj_list = new List<GameObjectBody>();
            // extracting the obj's so we don't get changed iterator bullshit (see http://www.co8.org/community/index.php?threads/holy-word-killed-my-cg-pc.12164/#post-145537)
            foreach (var target_item in spell.Targets)
            {
                // Only works on non-good creatures
                var target_item_obj = target_item.Object;
                var alignment = target_item_obj.GetAlignment();
                if (!(alignment.IsGood()))
                {
                    obj_list.Add(target_item_obj);
                }

                remove_list.Add(target_item_obj);
            }

            // for target_item in spell.target_list:
            // target_item_obj = target_item.obj
            foreach (var target_item_obj in obj_list)
            {
                Logger.Info("{0}", "target item: " + target_item_obj.ToString());
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item_obj);
                AttachParticles("sp-Holy Smite", target_item_obj);
                // Anything ten or more levels below the caster's level dies
                if (obj_hit_dice <= (spell.casterLevel - 10))
                {
                    if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item_obj)))
                    {
                        // print "Inflicting damage due to low critter HD: " + str(target_item_obj)
                        target_item_obj.Damage(SelectedPartyLeader, DamageType.PositiveEnergy, Dice.Parse("52d52"));
                    }

                    // So you'll get awarded XP for the kill
                    // print "Killing due to low critter HD: " + str(target_item_obj)
                    target_item_obj.Kill();
                }

                // Anything five or more levels below the caster's level is paralyzed
                if (obj_hit_dice <= (spell.casterLevel - 5))
                {
                    spell.duration = RandomRange(1, 10) * 10;
                    target_item_obj.AddCondition("sp-Hold Monster", spell.spellId, spell.duration, 0);
                }

                // Anything one or more levels below the caster's level is blinded
                if (obj_hit_dice <= (spell.casterLevel - 1))
                {
                    spell.duration = RandomRange(1, 4) + RandomRange(1, 4);
                    target_item_obj.AddCondition("sp-Blindness", spell.spellId, spell.duration, 0);
                }

                // Anything the caster's level or below is deafened
                if (obj_hit_dice <= (spell.casterLevel))
                {
                    spell.duration = RandomRange(1, 4);
                    target_item_obj.AddCondition("sp-Deafness", spell.spellId, spell.duration, 0);
                    // Summoned and extraplanar creatures below the caster's level are also banished
                    // if they fail a Will save at -4
                    if (target_item_obj.HasCondition(SpellEffects.SpellSummoned) || (target_item_obj.GetNpcFlags() & NpcFlag.EXTRAPLANAR) != 0)
                    {
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
                            if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item_obj)))
                            {
                                // print "Inflicting damage due to Summoned/Extraplanar: " + str(target_item_obj)
                                target_item_obj.Damage(SelectedPartyLeader, DamageType.PositiveEnergy, Dice.Parse("52d52"));
                            }

                            // So you'll get awarded XP for the kill
                            // print "critter_kill due to Summoned or Extraplanar: " + str(target_item_obj)
                            target_item_obj.Kill();
                        }

                    }

                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Holy Word OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Holy Word OnEndSpellCast");
        }

    }
}
