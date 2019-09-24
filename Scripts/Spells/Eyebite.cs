
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
    [SpellScript(161)]
    public class Eyebite : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Eyebite OnBeginSpellCast");
            Logger.Info("spell.target_list={0} id= {1}", spell.Targets, spell.spellId);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-cast", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Eyebite OnSpellEffect");
            spell.duration = 100 * spell.casterLevel; // 10 mins / caster level
            var targettingDuration = spell.Targets.Length - 1;
            // print "Applying slow to caster for ", targettingDuration
            spell.caster.AddCondition("sp-Slow", spell.spellId, targettingDuration, 0); // Only get single action.
            var time = 0;
            foreach (var target in spell.Targets)
            {
                if ((time == 0))
                {
                    biteTarget(target.Object, spell.caster, spell.spellId, spell.dc, spell.duration);
                }
                else
                {
                    // print "addimg timeevent for ", target.obj, " in ", time
                    StartTimer(time, () => biteTarget(target.Object, spell.caster, spell.spellId, spell.dc, spell.duration));
                }

                time = time + 1000;
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Eyebite OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Eyebite OnEndSpellCast");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Eyebite OnSpellStruck");
        }
        public void biteTarget(GameObjectBody target, GameObjectBody caster, int spell_id, int spell_dc, int duration)
        {
            Logger.Info("Eyebiting {0}", target);
            // Fortitude saving throw to negate
            if (target.SavingThrowSpell(spell_dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, caster, spell_id))
            {
                // saving throw successful
                target.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target);
            }
            else
            {
                // saving throw unsuccessful
                target.FloatMesFileLine("mes/spell.mes", 30002);
                Logger.Info("{0}is shaken", target); // shaken
                                                     // target.condition_add_with_args('sp-Emotion Despair', spell_id, duration, 0)
                target.AddCondition("sp-Doom", spell_id, duration, 0);
                if ((GameSystems.Critter.GetHitDiceNum(target) < 5))
                {
                    Logger.Info("{0}is comatose", target); // comatose
                                                           // target.condition_add_with_args( 'sp-Command', spell.id, spell.duration, 4 )
                    target.AddCondition("sp-Tashas Hideous Laughter", spell_id, duration, 0);
                }
                else if ((GameSystems.Critter.GetHitDiceNum(target) < 10))
                {
                    Logger.Info("{0}is panicked", target); // panicked
                    target.AddCondition("sp-Fear", spell_id, RandomRange(1, 4), 0);
                }

                AttachParticles("sp-Cause Fear", target);
            }

        }

    }
}
