
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
    [SpellScript(62)]
    public class ClairaudienceClairvoyance : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Clairaudience/Clairvoyance OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Clairaudience/Clairvoyance OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;

            var partsys_id = AttachParticles("sp-Clairaudience-Clairvoyance", spell.caster);

            spell.caster.AddCondition("sp-Clairaudience Clairvoyance", spell.spellId, spell.duration, partsys_id);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Clairaudience/Clairvoyance OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Clairaudience/Clairvoyance OnEndSpellCast");
        }


    }
}
