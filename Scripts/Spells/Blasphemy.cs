
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
    [SpellScript(36)]
    public class Blasphemy : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Blasphemy OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Blasphemy OnSpellEffect");
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

            foreach (var target_item in spell.Targets)
            {
                var obj_hit_dice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
                // Only works on non-evil creatures
                var alignment = target_item.Object.GetAlignment();
                if (!(alignment.IsEvil()) && !(npc == target_item.Object))
                {
                    AttachParticles("sp-Slay Living", target_item.Object);
                    // Anything ten or more levels below the caster's level dies
                    if (obj_hit_dice <= (npc_caster_level - 10))
                    {
                        // So you'll get awarded XP for the kill
                        if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                        {
                            target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                        }

                        target_item.Object.Kill();
                    }

                    // Anything five or more levels below the caster's level is paralyzed
                    if (obj_hit_dice <= (npc_caster_level - 5))
                    {
                        spell.duration = RandomRange(1, 10) * 10;
                        target_item.Object.AddCondition("sp-Hold Monster", spell.spellId, spell.duration, 0);
                    }

                    // Anything one or more levels below the caster's level is weakened
                    if (obj_hit_dice <= (npc_caster_level - 1))
                    {
                        spell.duration = RandomRange(1, 4) + RandomRange(1, 4);
                        var dam_amount = RandomRange(1, 6) + RandomRange(1, 6);
                        target_item.Object.AddCondition("sp-Ray of Enfeeblement", spell.spellId, spell.duration, dam_amount);
                    }

                    // Anything the caster's level or below is dazed
                    if (obj_hit_dice <= (npc_caster_level))
                    {
                        spell.duration = 1;
                        target_item.Object.AddCondition("sp-Daze", spell.spellId, spell.duration, 0);
                        // Summoned and extraplanar creatures below the caster's level are also banished
                        // if they fail a Will save at -4
                        if (target_item.Object.HasCondition(SpellEffects.SpellSummoned) || (target_item.Object.GetNpcFlags() & NpcFlag.EXTRAPLANAR) != 0)
                        {
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
                                // creature is sent back to its own plane
                                // kill for now
                                target_item.Object.Kill();
                            }

                        }

                    }

                }

                remove_list.Add(target_item.Object);
            }

            // f.close()
            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Blasphemy OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Blasphemy OnEndSpellCast");
        }

    }
}
