
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
    [SpellScript(128)]
    public class Dismissal : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dismissal OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Dismissal OnSpellEffect");
            var target_item = spell.Targets[0];
            spell.duration = 0;
            // spell.dc is DC - target's HD + caster's caster_level
            spell.dc = spell.dc - GameSystems.Critter.GetHitDiceNum(target_item.Object) + spell.casterLevel;
            spell.dc = Math.Max(1, spell.dc);
            if ((target_item.Object.type == ObjectType.npc))
            {
                if (target_item.Object.HasCondition(SpellEffects.SpellSummoned) || target_item.Object.IsMonsterCategory(MonsterCategory.outsider))
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
                        // creature is sent back to its own plane (no 20% of wrong plane, DUMB)
                        // kill for now
                        // So you'll get awarded XP for the kill
                        if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                        {
                            target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                        }

                        target_item.Object.Kill();
                        AttachParticles("sp-Dismissal", target_item.Object);
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

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Dismissal OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dismissal OnEndSpellCast");
        }

    }
}
