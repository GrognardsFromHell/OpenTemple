
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
            // it takes 1 round to pull the target to the frog (normally)
            spell.duration = 0;
            var target_item = spell.Targets[0];
            // if the target is larger than the frog, it takes 2 turns to "pull" the target in
            if (GameSystems.Stat.DispatchGetSizeCategory(target_item.Object) > GameSystems.Stat.DispatchGetSizeCategory(spell.caster))
            {
                spell.duration = 1;
            }

            if (spell.caster.PerformTouchAttack(target_item.Object) == D20CAF.HIT)
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 21000);
                // hit
                // target_item.obj.condition_add_with_args( 'sp-Frog Tongue', spell.id, spell.duration, 0 )
                spell.caster.AddCondition("sp-Frog Tongue", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-Frog Tongue", target_item.Object);
            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 21001);
                spell.caster.StartFrogGrapplePhase(FrogGrapplePhase.FailedLatch);
                // missed
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
