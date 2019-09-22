
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
    [SpellScript(123)]
    public class DimensionDoor : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnSpellEffect");
            AttachParticles("sp-Dimension Door", spell.caster);
            var target = spell.caster;

            if (!target.HasCondition(SpellEffects.SpellDimensionalAnchor))
            {
                target.FadeTo(0, 10, 40);
                AttachParticles("sp-Dimension Door", target);
                StartTimer(750, () => fade_back_in(target, spell.aoeCenter, spell), true);
            }
            else
            {
                target.FloatMesFileLine("mes/spell.mes", 30011);
                AttachParticles("Fizzle", target);
                spell.RemoveTarget(target);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dimension Door OnEndSpellCast");
        }
        public static void fade_back_in(GameObjectBody target, LocAndOffsets loc, SpellPacketBody spell)
        {
            target.Move(loc);
            AttachParticles("sp-Dimension Door", target);
            target.FadeTo(255, 10, 5);
            spell.RemoveTarget(target);
            spell.EndSpell();
        }


    }
}
