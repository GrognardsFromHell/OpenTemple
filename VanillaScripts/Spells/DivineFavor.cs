
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(137)]
    public class DivineFavor : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Divine Favor OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Divine Favor OnSpellEffect");
            int bonus; // DECL_PULL_UP
            if (spell.casterLevel >= 12)
            {
                bonus = 4;

            }
            else if (spell.casterLevel >= 9)
            {
                bonus = 3;

            }
            else if (spell.casterLevel >= 6)
            {
                bonus = 2;

            }
            else
            {
                bonus = 1;

            }

            spell.duration = 10;

            var target = spell.Targets[0];

            target.Object.AddCondition("sp-Divine Favor", spell.spellId, spell.duration, bonus);
            target.ParticleSystem = AttachParticles("sp-Divine Favor", target.Object);

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Divine Favor OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Divine Favor OnEndSpellCast");
        }


    }
}
