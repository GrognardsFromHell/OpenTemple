
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
    [SpellScript(775)]
    public class Resonance : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Resonance OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Resonance OnSpellEffect");
            var target_item = spell.Targets[0];
            var caster = spell.caster;
            var isValid = false;
            if (caster.GetStat(Stat.level_wizard) >= 1)
            {
                caster.PendingSpellsToMemorized();
                isValid = true;
            }

            if (caster.GetStat(Stat.level_bard) >= 1)
            {
                caster.ResetCastSpells(Stat.level_bard);
                isValid = true;
            }

            if (caster.GetStat(Stat.level_sorcerer) >= 1)
            {
                caster.ResetCastSpells(Stat.level_sorcerer);
                isValid = true;
            }

            if (isValid)
            {
                Logger.Info("doing spells pending to memorized for spell caster");
                AttachParticles("sp-Read Magic", caster);
            }
            else
            {
                AttachParticles("Fizzle", caster);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Resonance OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Resonance OnEndSpellCast");
        }

    }
}
