
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(50)]
    public class CauseFear : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cause Fear OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Cause Fear OnSpellEffect");
            var dice = Dice.D4;

            spell.duration = dice.Roll();

            var target_item = spell.Targets[0];

            if ((GameSystems.Critter.GetHitDiceNum(target_item.Object) < 6))
            {
                if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    var return_val = target_item.Object.AddCondition("sp-Cause Fear", spell.spellId, spell.duration, 1);

                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Cause Fear", target_item.Object);

                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    var return_val = target_item.Object.AddCondition("sp-Cause Fear", spell.spellId, spell.duration, 0);

                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Cause Fear", target_item.Object);

                    }

                }

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Cause Fear OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cause Fear OnEndSpellCast");
        }


    }
}
