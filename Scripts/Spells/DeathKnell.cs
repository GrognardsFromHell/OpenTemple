
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
    [SpellScript(100)]
    public class DeathKnell : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Death Knell OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Death Knell OnSpellEffect");
            var target_item = spell.Targets[0];
            var npc = spell.caster; // added so NPC's will choose valid targets
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    var curr = pc.GetStat(Stat.hp_current);
                    if ((curr <= 0 && curr >= -9 && pc.DistanceTo(npc) <= 10))
                    {
                        target_item.Object = pc;
                    }

                }

            }

            if (npc.GetNameId() == 14609)
            {
                spell.casterLevel = 8;
            }

            if (npc.GetNameId() == 14601)
            {
                spell.casterLevel = 4;
            }

            spell.duration = 100 * GameSystems.Critter.GetHitDiceNum(target_item.Object);
            if (target_item.Object.GetStat(Stat.hp_current) < 0)
            {
                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // target_item.obj.condition_add_with_args( 'sp-Death Knell', spell.id, spell.duration, 2 )
                    spell.caster.FloatMesFileLine("mes/spell.mes", 20023);
                    spell.caster.AddCondition("sp-Death Knell", spell.spellId, spell.duration, 2); // 2 is STR bonus
                                                                                                   // So you'll get awarded XP for the kill
                    if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                    {
                        target_item.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                    }

                    target_item.Object.KillWithDeathEffect();
                    target_item.ParticleSystem = AttachParticles("sp-Death Knell", target_item.Object);
                }
                else
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }
            else
            {
                // target's hp_cur is >= 0
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31006);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Death Knell OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Death Knell OnEndSpellCast");
        }

    }
}
