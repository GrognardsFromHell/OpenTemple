
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
    [SpellScript(711)]
    public class PotionOfProtectionFromFire : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnSpellEffect");
            spell.duration = spell.casterLevel * 100;

            int protection_pts; // DECL_PULL_UP
            if ((spell.casterLevel < 10))
            {
                protection_pts = spell.casterLevel * 12;

            }
            else
            {
                protection_pts = 120;

            }

            var target = spell.Targets[0];

            target.Object.AddCondition("sp-Potion of protection from energy", spell.spellId, spell.duration, DamageType.Fire, protection_pts);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnEndSpellCast");
        }


    }
}
