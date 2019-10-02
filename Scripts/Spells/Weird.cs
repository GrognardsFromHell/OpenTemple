
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
    [SpellScript(532)]
    public class Weird : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Weird OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-illusion-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Weird OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var dice = Dice.Parse("3d6");
            foreach (var target in spell.Targets)
            {
                remove_list.Add(target.Object);
                AttachParticles("sp-Phantasmal Killer", target.Object);
                // will save to negate, otherwise fort save to avoid death
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target.Object);
                }
                else if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR, spell.caster, spell.spellId))
                {
                    // fort save success, damage 3d6
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    // saving throw unsuccesful, damage target, full damage
                    target.Object.DealSpellDamage(spell.caster, DamageType.Unspecified, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }
                else
                {
                    // fort save failure, kill
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // kill target
                    // So you'll get awarded XP for the kill
                    if (!((SelectedPartyLeader.GetPartyMembers()).Contains(target.Object)))
                    {
                        target.Object.Damage(SelectedPartyLeader, DamageType.Unspecified, Dice.Parse("1d1"));
                    }

                    target.Object.Kill();
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Weird OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Weird OnEndSpellCast");
        }

    }
}