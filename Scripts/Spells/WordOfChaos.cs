
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

[SpellScript(539)]
public class WordOfChaos : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Word of Chaos OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Word of Chaos OnSpellEffect");
        var remove_list = new List<GameObject>();
        // The Will save versus banishment is at a -4 penalty
        spell.dc = spell.dc + 4;
        var npc = spell.caster;
        var npc_caster_level = spell.casterLevel;
        if (npc.GetNameId() == 14286 || npc.GetNameId() == 14358) // Balors
        {
            npc_caster_level = 20;
            spell.dc = 25 + 4; // only affects banishment anyway
        }

        // f = open('wordofchaos_feedback.txt', 'w')
        // f.write( 'ccc\n' )
        var target_list = Utilities.pyspell_targetarray_copy_to_obj_list(spell);
        foreach (var target_item_obj in target_list)
        {
            // f.write( str(target_item_obj.name) + '\n' )
            var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item_obj);
            var is_confused = 0;
            // Only works on non-chaotic creatures
            var alignment = target_item_obj.GetAlignment();
            if (!(alignment.IsChaotic()) && !(npc == target_item_obj))
            {
                AttachParticles("sp-Polymorph Other", target_item_obj);
                // Anything ten or more levels below the caster's level dies
                if (obj_hit_dice <= (npc_caster_level - 10))
                {
                    // So you'll get awarded XP for the kill
                    if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item_obj)))
                    {
                        target_item_obj.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                    }

                    target_item_obj.Kill();
                }

                // Anything five or more levels below the caster's level is confused
                if (obj_hit_dice <= (npc_caster_level - 5))
                {
                    spell.duration = RandomRange(1, 10) * 10;
                    target_item_obj.FloatMesFileLine("mes/combat.mes", 113, TextFloaterColor.Red);
                    target_item_obj.AddCondition("sp-Confusion", spell.spellId, spell.duration, 0);
                    is_confused = 1; // added because Confusion will end when either the stun or deafness spell end (Confusion needs a fix.)
                }

                // Anything one or more levels below the caster's level is stunned
                if (obj_hit_dice <= (npc_caster_level - 1))
                {
                    if (is_confused == 0) // added because Confusion will end when the stun spell ends
                    {
                        spell.duration = 0;
                        target_item_obj.AddCondition("sp-Color Spray Stun", spell.spellId, spell.duration, 0);
                    }

                }

                // Anything the caster's level or below is deafened
                if (obj_hit_dice <= (npc_caster_level))
                {
                    if (is_confused == 0) // added because Confusion will end when the deafness spell ends
                    {
                        spell.duration = RandomRange(1, 4);
                        target_item_obj.AddCondition("sp-Shout", spell.spellId, spell.duration, 0);
                    }

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

        // f.close()
        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Word of Chaos OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Word of Chaos OnEndSpellCast");
    }

}