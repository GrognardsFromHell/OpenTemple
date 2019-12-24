
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
