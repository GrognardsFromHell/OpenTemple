
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

namespace VanillaScripts
{
    [SpellScript(600)]
    public class FrogTongue : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Frog Tongue OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Frog Tongue OnSpellEffect");
            spell.duration = 0;

            var target_item = spell.Targets[0];

            if (GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) > GameSystems.Stat.DispatchGetSizeCategory(spell.caster))
            {
                spell.duration = 1;

            }

            if (spell.caster.PerformTouchAttack(target_item.Object) == D20CAF.HIT)
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 21000);
                spell.caster.AddCondition("sp-Frog Tongue", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-Frog Tongue", target_item.Object);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 21001);
                spell.caster.StartFrogGrapplePhase(FrogGrapplePhase.FailedLatch);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Frog Tongue OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Frog Tongue OnEndSpellCast");
        }


    }
}
